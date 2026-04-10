# DotBoil.Cronos

Cron ifadesi tabanlı zamanlanmış görev yöneticisi. `ICronosJob` arayüzünü implement eden işler, `appsettings.json` üzerinden cron zamanlamasıyla tanımlanır. Görevler MySQL'de izlenir, `HostedService` ile arka planda çalışır.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Cronos\DotBoil.Cronos.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Cronos": {
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=MyApp"
      },
      "Jobs": [
        {
          "Name": "DailyReport",
          "TypeName": "MyApp.Jobs.DailyReportJob",
          "CronExpression": "0 8 * * *",
          "TimeZone": "Europe/Istanbul"
        },
        {
          "Name": "CacheCleanup",
          "TypeName": "MyApp.Jobs.CacheCleanupJob",
          "CronExpression": "*/30 * * * *",
          "TimeZone": "UTC"
        }
      ]
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Persistence.ConnectionString` | `string` | MySQL bağlantı dizesi (görev kayıtları için) |
| `Jobs[].Name` | `string` | Görevin benzersiz adı |
| `Jobs[].TypeName` | `string` | `ICronosJob` implement eden sınıfın tam adı |
| `Jobs[].CronExpression` | `string` | Cron zamanlaması ifadesi |
| `Jobs[].TimeZone` | `string` | Zaman dilimi (IANA formatında, ör: `Europe/Istanbul`) |

---

## Job Tanımlama

`ICronosJob` arayüzünü implement edin:

```csharp
public class DailyReportJob : ICronosJob
{
    public string Name => "DailyReport";

    private readonly IRepository<Order, AppDbContext> _orderRepository;
    private readonly IMailSender _mailSender;
    private readonly ILogger<DailyReportJob> _logger;

    public DailyReportJob(
        IRepository<Order, AppDbContext> orderRepository,
        IMailSender mailSender,
        ILogger<DailyReportJob> logger)
    {
        _orderRepository = orderRepository;
        _mailSender      = mailSender;
        _logger          = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Günlük rapor görevi başladı.");

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var orders = await _orderRepository.Get()
            .Where(o => o.CreatedAt.Date == yesterday)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Dün {Count} sipariş alındı.", orders.Count);

        // Rapor gönder...
    }
}
```

---

## ICronosJob Arayüzü

```csharp
public interface ICronosJob
{
    // appsettings.json Jobs[].Name değeriyle eşleşmeli
    string Name { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
```

---

## Cron İfadesi Referansı

```
┌───── dakika    (0-59)
│ ┌───── saat   (0-23)
│ │ ┌───── gün  (1-31)
│ │ │ ┌───── ay (1-12)
│ │ │ │ ┌───── haftanın günü (0-7, 0 ve 7 = Pazar)
│ │ │ │ │
* * * * *
```

| İfade | Açıklama |
|-------|----------|
| `* * * * *` | Her dakika |
| `*/30 * * * *` | Her 30 dakikada bir |
| `0 * * * *` | Her saat başı |
| `0 8 * * *` | Her gün sabah 08:00 |
| `0 8 * * 1` | Her Pazartesi sabah 08:00 |
| `0 0 1 * *` | Her ayın 1'i gece yarısı |
| `0 0 1 1 *` | Her yılın 1 Ocak günü |

---

## Kayıtlı Servisler

| Servis | Yaşam Süresi |
|--------|--------------|
| `CronosJobHostedService` | Singleton (HostedService) |
| `IScheduledJobRepository` | Scoped |
| `CronosDbContext` | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Cronos 0.7.2`
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`