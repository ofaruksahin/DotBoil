# DotBoil.Caching

Redis tabanlı dağıtık önbellekleme modülü. `ICache` arayüzü üzerinden tip-güvenli get/set/remove operasyonları sağlar.

---

## Kurulum

Projeye referans ekleyin:

```xml
<ProjectReference Include="..\DotBoil.Caching\DotBoil.Caching.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Caching": {
      "Endpoints": [
        { "Ip": "127.0.0.1", "Port": 6379 }
      ],
      "Password": ""
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Endpoints` | `List` | Redis node listesi. Cluster için birden fazla eklenebilir. |
| `Endpoints[].Ip` | `string` | Redis sunucu IP adresi |
| `Endpoints[].Port` | `int` | Redis sunucu portu (varsayılan: 6379) |
| `Password` | `string` | Redis auth şifresi (boş bırakılabilir) |

---

## Kullanım

`ICache` servisini inject edin:

```csharp
public class ProductService
{
    private readonly ICache _cache;

    public ProductService(ICache cache)
    {
        _cache = cache;
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        var cacheKey = $"product:{id}";

        // Önce cache'e bak, yoksa veritabanından çek ve cache'e yaz
        return await _cache.GetOrSetAsync(
            cacheKey,
            async () => await _db.Products.FindAsync(id),
            expire: TimeSpan.FromMinutes(30)
        );
    }

    public async Task InvalidateProductAsync(int id)
    {
        await _cache.RemoveAsync($"product:{id}");
    }
}
```

---

## ICache Arayüzü

```csharp
public interface ICache
{
    // Anahtarın cache'de var olup olmadığını kontrol eder
    Task<bool> KeyExistsAsync(string key);

    // Değeri cache'e yazar
    Task SetAsync<T>(string key, T value, TimeSpan? expire = default);

    // Cache'de varsa döner; yoksa action'ı çalıştırıp sonucu cache'e yazar
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default);

    // Anahtarı cache'den siler
    Task RemoveAsync(string key);
}
```

---

## Örnekler

### Basit Değer Saklama

```csharp
// Yazma
await _cache.SetAsync("app:config:theme", "dark", TimeSpan.FromHours(24));

// Okuma (GetOrSet pattern)
var theme = await _cache.GetOrSetAsync(
    "app:config:theme",
    () => Task.FromResult("light"),  // default değer
    TimeSpan.FromHours(24)
);
```

### Nesne Saklama

```csharp
public record UserSession(string UserId, string[] Roles, DateTime ExpiresAt);

var session = new UserSession("user-123", new[] { "Admin" }, DateTime.UtcNow.AddHours(1));
await _cache.SetAsync($"session:{session.UserId}", session, TimeSpan.FromHours(1));
```

### Cache Varlık Kontrolü

```csharp
if (await _cache.KeyExistsAsync($"rate-limit:{userId}"))
{
    throw new TooManyRequestsException();
}
await _cache.SetAsync($"rate-limit:{userId}", true, TimeSpan.FromSeconds(60));
```

### Cache Geçersiz Kılma (Invalidation)

```csharp
// Ürün güncellendiğinde cache'i temizle
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    await _cache.RemoveAsync($"product:{product.Id}");
    await _cache.RemoveAsync("products:list"); // Liste cache'ini de temizle
}
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `ICache` | `RedisCache` | Singleton |
| `IConnectionMultiplexer` | StackExchange.Redis | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `StackExchange.Redis 2.10.1`
