# DotBoil.EFCore

Entity Framework Core entegrasyonu. Repository pattern, audit interceptor ve dinamik DbContext yükleme altyapısı sağlar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.EFCore\DotBoil.EFCore.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "EFCore": {
      "Contexts": [
        {
          "TypeName": "MyApp.Infrastructure.Data.AppDbContext",
          "Interceptors": [
            "DotBoil.EFCore.Interceptors.AuditInterceptor"
          ]
        }
      ]
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Contexts[].TypeName` | `string` | DbContext'in tam nitelikli tip adı (assembly qualified) |
| `Contexts[].Interceptors` | `string[]` | Uygulanacak interceptor'ların tam tip adları |

---

## DbContext Oluşturma

`EFCoreDbContext` base sınıfını kalıtın:

```csharp
public class AppDbContext : EFCoreDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
```

### DbContext Loader

EFCore modülüne context'in nasıl oluşturulacağını bildirmek için `EFCoreDbContextLoader<T>` sınıfını implement edin:

```csharp
internal class AppDbContextLoader : EFCoreDbContextLoader<AppDbContext>
{
    public override void Load(DbContextOptionsBuilder<AppDbContext> builder)
    {
        var options = DotBoilApp.Configuration.GetConfigurations<AppDbContextOptions>();
        builder.UseMySql(
            options.ConnectionString,
            ServerVersion.AutoDetect(options.ConnectionString)
        );
    }
}
```

```csharp
internal class AppDbContextOptions : IOptions
{
    public string Key => "DotBoil:MyApp:DbContext";
    public string ConnectionString { get; set; } = string.Empty;
}
```

---

## Entity Tanımlama

Tüm entity'ler `BaseEntity`'den türemelidir:

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

### Entity Configuration

```csharp
internal class ProductConfiguration : EFCoreEntityTypeConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}
```

---

## Repository Pattern

`IRepository<TEntity, TContext>` arayüzünü inject edin:

```csharp
public class ProductService
{
    private readonly IRepository<Product, AppDbContext> _repository;

    public ProductService(IRepository<Product, AppDbContext> repository)
    {
        _repository = repository;
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public IQueryable<Product> GetActiveProducts()
        => _repository.Get().Where(p => p.StockQuantity > 0);

    public async Task CreateAsync(Product product)
    {
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdatePriceAsync(int id, decimal newPrice)
    {
        var product = await _repository.GetByIdAsync(id);
        product.Price = newPrice;
        _repository.Update(product);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        _repository.Remove(product);
        await _repository.SaveChangesAsync();
    }
}
```

---

## IRepository Arayüzü

```csharp
public interface IRepository<TEntity, TContext>
    where TEntity : BaseEntity
    where TContext : EFCoreDbContext
{
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    Task<TEntity> GetByIdAsync(int id);
    IQueryable<TEntity> Get();         // Soft-delete filtrelenmiş kayıtlar
    IQueryable<TEntity> GetAll();      // Tüm kayıtlar (silinmişler dahil)
    Task SaveChangesAsync();
}
```

---

## Audit Interceptor

`AuditInterceptor` konfigürasyona eklendiğinde, `IAuditUser` servisini kullanarak `CreatedAt`, `UpdatedAt` ve `CreatedBy`, `UpdatedBy` alanlarını otomatik doldurur.

`IAuditUser` arayüzünü uygulayarak mevcut kullanıcı bilgisini sağlayın:

```csharp
internal class CurrentAuditUserService : IAuditUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentAuditUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : null;
    }
}
```

---

## Migration

```bash
# Migration oluşturma
dotnet ef migrations add InitialCreate --project MyApp.Infrastructure --startup-project MyApp.Api

# Migration uygulama
dotnet ef database update --project MyApp.Infrastructure --startup-project MyApp.Api
```

---

## Kayıtlı Servisler

| Servis | Yaşam Süresi |
|--------|--------------|
| `IRepository<TEntity, TContext>` | Scoped |
| `TContext` (DbContext'ler) | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Microsoft.EntityFrameworkCore 10.0.2`
- `Microsoft.EntityFrameworkCore.Relational 10.0.2`