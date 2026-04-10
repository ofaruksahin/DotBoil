# DotBoil.Localization

Redis + MySQL tabanlı çoklu dil desteği. Çeviri metinleri veritabanında saklanır, Redis üzerinden önbelleğe alınır. `ILocalize` arayüzü ile çeviri metinlerine dil ve grup bazlı erişim sağlar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Localization\DotBoil.Localization.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Localization": {
      "Caching": {
        "ConnectionString": "127.0.0.1:6379,password=mypassword",
        "ExpireInHour": 12
      },
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=MyApp"
      }
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Caching.ConnectionString` | `string` | Redis bağlantı dizesi |
| `Caching.ExpireInHour` | `int` | Cache süresinin son bulacağı saat |
| `Persistence.ConnectionString` | `string` | MySQL bağlantı dizesi |

---

## Kullanım

```csharp
public class ProductService
{
    private readonly ILocalize _localize;

    public ProductService(ILocalize localize)
    {
        _localize = localize;
    }

    public async Task<string> GetNotFoundMessageAsync()
    {
        // Tek anahtar ile
        return await _localize.LocalizeText("product_not_found");
    }

    public async Task<string> GetValidationErrorAsync()
    {
        // Grup + anahtar ile
        return await _localize.LocalizeText("Validation", "required_field");
    }

    public async Task<string> GetMessageInLanguageAsync(string language)
    {
        // Belirli bir dilde
        return await _localize.LocalizeTextWithLanguage(language, "welcome_message");
    }
}
```

---

## ILocalize Arayüzü

```csharp
public interface ILocalize
{
    // Aktif dilde anahtar ile çeviri getirir
    Task<string> LocalizeText(string name);

    // Aktif dilde grup + anahtar ile çeviri getirir
    Task<string> LocalizeText(string group, string name);

    // Belirtilen dilde anahtar ile çeviri getirir
    Task<string> LocalizeTextWithLanguage(string language, string name);

    // Belirtilen dilde grup + anahtar ile çeviri getirir
    Task<string> LocalizeTextWithLanguage(string language, string group, string name);
}
```

---

## API Endpoint'leri

Modül yüklendiğinde aşağıdaki endpoint'ler otomatik olarak eklenir:

| Method | Path | Açıklama |
|--------|------|----------|
| `GET` | `/localization` | Tüm çeviri anahtarlarını listele |
| `GET` | `/localization/{language}` | Belirli dildeki tüm çevirileri getir |
| `POST` | `/localization` | Yeni çeviri anahtarı ekle |
| `PUT` | `/localization/{id}` | Çeviriyi güncelle |
| `DELETE` | `/localization/{id}` | Çeviriyi sil |

---

## Mevcut Dil Yönetimi

`ICurrentLanguage` arayüzü ile request bazlı aktif dili yönetin:

```csharp
internal class CurrentLanguageService : ICurrentLanguage
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentLanguageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentLanguage()
    {
        // Accept-Language header'ından veya JWT claim'inden okuyabilirsiniz
        return _httpContextAccessor.HttpContext?
            .Request.Headers["Accept-Language"].FirstOrDefault() ?? "TR";
    }
}
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `ILocalize` | `Localize` | Singleton |
| `LocalizationDbContext` | — | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`
- `StackExchange.Redis`
