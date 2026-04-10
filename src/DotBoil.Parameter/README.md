# DotBoil.Parameter

Redis + MySQL tabanlı uygulama parametre yönetimi. Parametreler veritabanında saklanır, Redis üzerinden önbelleğe alınır. `IParameterManager` ile tip-güvenli parametre okuma, tenant bazlı ve genel parametreler desteklenir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Parameter\DotBoil.Parameter.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Parameters": {
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
| `Caching.ExpireInHour` | `int` | Cache geçerlilik süresi (saat) |
| `Persistence.ConnectionString` | `string` | MySQL bağlantı dizesi |

---

## Kullanım

```csharp
public class NotificationService
{
    private readonly IParameterManager _parameters;

    public NotificationService(IParameterManager parameters)
    {
        _parameters = parameters;
    }

    public async Task SendAsync(string userId)
    {
        // Section + name ile
        var smtpHost = await _parameters.GetParameterValue<string>("Smtp", "Host");
        var smtpPort = await _parameters.GetParameterValue<int>("Smtp", "Port");

        // Sadece name ile
        var maxRetry = await _parameters.GetParameterValue<int>("MaxRetryCount");

        // Public parametreler (yetki gerekmez)
        var appName = await _parameters.GetParameterValue<string>("AppName", isPublic: true);
    }
}
```

---

## IParameterManager Arayüzü

```csharp
public interface IParameterManager
{
    // Genel parametre — section + name
    Task<T> GetParameterValue<T>(string section, string name, bool isPublic = false);

    // Genel parametre — sadece name
    Task<T> GetParameterValue<T>(string name, bool isPublic = false);

    // Tenant bazlı parametre — tenantId + name
    Task<T> GetParameterValue<T>(int tenantId, string name, bool isPublic = false);

    // Tenant bazlı parametre — tenantId + section + name
    Task<T> GetParameterValue<T>(int tenantId, string section, string name, bool isPublic = false);
}
```

| Parametre | Açıklama |
|-----------|----------|
| `T` | Parametre değerinin dönüştürüleceği tip (string, int, bool, vb.) |
| `section` | Parametrenin grubu (ör: `"Smtp"`, `"Payment"`) |
| `name` | Parametre adı (ör: `"Host"`, `"ApiKey"`) |
| `isPublic` | `true` ise yetkilendirme kontrolü atlanır |
| `tenantId` | Çok kiracılı mimaride kiracıya özel parametre |

---

## API Endpoint'leri

Modül yüklendiğinde aşağıdaki endpoint'ler otomatik eklenir:

| Method | Path | Açıklama |
|--------|------|----------|
| `GET` | `/parameters` | Tüm parametreleri listele |
| `GET` | `/parameters/{id}` | Belirli parametreyi getir |
| `POST` | `/parameters` | Yeni parametre ekle |
| `PUT` | `/parameters/{id}` | Parametreyi güncelle |
| `DELETE` | `/parameters/{id}` | Parametreyi sil |

---

## Tip Dönüşümleri

```csharp
// string
var apiUrl = await _parameters.GetParameterValue<string>("ExternalApi", "BaseUrl");

// int
var pageSize = await _parameters.GetParameterValue<int>("Pagination", "DefaultPageSize");

// bool
var maintenanceMode = await _parameters.GetParameterValue<bool>("MaintenanceMode");

// decimal
var taxRate = await _parameters.GetParameterValue<decimal>("Tax", "Rate");
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `IParameterManager` | `ParameterManager` | Singleton |
| `ParameterDbContext` | — | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`
- `StackExchange.Redis`