# DotBoil.Health

ASP.NET Core Health Checks altyapısını ve görsel Health Check UI'ını yapılandırır. Bağımlılıkların (veritabanı, Redis, RabbitMQ vb.) sağlık durumunu tek bir endpoint'ten izlemenizi sağlar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Health\DotBoil.Health.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Health": {
      "Url": "/health",
      "UI": {
        "InMemory": {
          "Enabled": true
        },
        "Services": [
          {
            "Name": "My API",
            "Url": "https://localhost:7001/health"
          }
        ]
      }
    }
  }
}
```

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `Url` | `string` | `/health` | Health check JSON endpoint yolu |
| `UI.InMemory.Enabled` | `bool` | `false` | Health check geçmişini bellekte sakla |
| `UI.Services` | `List` | — | UI'da görüntülenecek servis listesi |
| `UI.Services[].Name` | `string` | — | Servise verilen görünen ad |
| `UI.Services[].Url` | `string` | — | Servisin `/health` endpoint URL'i |

### MySQL Persistence (Production)

```json
{
  "DotBoil": {
    "Health": {
      "Url": "/health",
      "UI": {
        "MySql": {
          "ConnectionString": "server=localhost;uid=root;pwd=pass;database=HealthChecks"
        },
        "Services": [
          { "Name": "Auth API", "Url": "https://auth.example.com/health" },
          { "Name": "Order API", "Url": "https://orders.example.com/health" }
        ]
      }
    }
  }
}
```

---

## Özel Health Check Ekleme

Kendi sağlık kontrolünüzü `IHealthCheck` ile tanımlayın ve `ConfigureHealthCheck` üzerinden kaydedin:

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public DatabaseHealthCheck(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Veritabanı bağlantısı sağlıklı.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Veritabanı bağlantısı başarısız.", ex);
        }
    }
}
```

```csharp
internal class AppHealthCheck : ConfigureHealthCheck
{
    public override void Configure(IHealthChecksBuilder builder)
    {
        builder
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db" })
            .AddRedis("127.0.0.1:6379", "redis", tags: new[] { "cache" })
            .AddRabbitMQ("amqp://guest:guest@localhost:5672", name: "rabbitmq");
    }
}
```

---

## Endpoint'ler

| Endpoint | Açıklama |
|----------|----------|
| `GET /health` | JSON formatında sağlık durumu |
| `GET /healthui` | Görsel Health Check UI |

### Örnek `/health` Yanıtı

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "Veritabanı bağlantısı sağlıklı.",
      "duration": "00:00:00.0100234"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0050123"
    }
  }
}
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `AspNetCore.HealthChecks.UI`
- `AspNetCore.HealthChecks.UI.Client`
- `AspNetCore.HealthChecks.UI.InMemory.Storage`
- `AspNetCore.HealthChecks.UI.MySql.Storage`