# DotBoil.Mapper

AutoMapper entegrasyonu. `Profile` sınıflarını otomatik keşfeder ve kaydeder; manuel kayıt gerekmez.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Mapper\DotBoil.Mapper.csproj" />
```

---

## Konfigürasyon

Ekstra konfigürasyon gerekmez. Modül, çalışma zamanında `Profile`'dan türeyen tüm sınıfları otomatik bulur.

---

## Kullanım

### Profile Tanımlama

```csharp
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<Product, ProductDetailDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

### IMapper Inject Etme

```csharp
public class ProductService
{
    private readonly IMapper _mapper;
    private readonly IRepository<Product, AppDbContext> _repository;

    public ProductService(IMapper mapper, IRepository<Product, AppDbContext> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _repository.Get().ToListAsync();
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task CreateAsync(CreateProductRequest request)
    {
        var product = _mapper.Map<Product>(request);
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
    }
}
```

---

## Koleksiyon Mapping

```csharp
// List → List
var productDtos = _mapper.Map<List<ProductDto>>(products);

// IQueryable üzerinde ProjectTo (N+1 sorgu problemini önler)
var productDtos = await _repository.Get()
    .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
    .ToListAsync();
```

---

## Özel Dönüşümler

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.TotalAmount,
                opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity)))
            .ForMember(dest => dest.StatusText,
                opt => opt.MapFrom(src => src.Status.GetDisplayName()))
            .ForMember(dest => dest.CustomerFullName,
                opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));
    }
}
```

---

## Kayıtlı Servisler

| Servis | Yaşam Süresi |
|--------|--------------|
| `IMapper` | Singleton |
| `IConfigurationProvider` | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `AutoMapper 16.0.0`