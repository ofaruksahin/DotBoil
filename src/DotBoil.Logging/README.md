# DotBoil.Logging

Serilog tabanlı loglama altyapısı. Sink sistemi sayesinde console, dosya ve özel hedeflere loglama yapılabilir. Yeni sink'ler konfigürasyon değişikliği olmadan eklenebilir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Logging\DotBoil.Logging.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Logging": {
      "Sinks": [
        {
          "Key": "DotBoil:Logging:Console"
        },
        {
          "Key": "DotBoil:Logging:File",
          "LogFileName": "logs/myapp",
          "RollingInterval": "Day"
        }
      ]
    }
  }
}
```

### Console Sink

```json
{
  "Key": "DotBoil:Logging:Console"
}
```

### File Sink

```json
{
  "Key": "DotBoil:Logging:File",
  "LogFileName": "logs/myapp",
  "RollingInterval": "Day"
}
```

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `LogFileName` | `string` | — | Log dosyasının yolu ve adı (uzantı otomatik eklenir) |
| `RollingInterval` | `string` | `Day` | Log dosyası döngüsü: `Infinite`, `Year`, `Month`, `Day`, `Hour`, `Minute` |

---

## Kullanım

`Microsoft.Extensions.Logging.ILogger<T>` inject edin:

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Sipariş oluşturuluyor. UserId: {UserId}", request.UserId);

        try
        {
            // İş mantığı...
            _logger.LogInformation("Sipariş oluşturuldu. OrderId: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş oluşturulurken hata. UserId: {UserId}", request.UserId);
            throw;
        }
    }
}
```

---

## Özel Sink Ekleme

`ISink` arayüzünü implement ederek özel sink'ler tanımlayabilirsiniz:

```csharp
internal class ElasticsearchSinkOptions : ISink
{
    public string Key => "DotBoil:Logging:Elasticsearch";
    public string NodeUri { get; set; } = string.Empty;
    public string IndexFormat { get; set; } = "logs-{0:yyyy.MM.dd}";
}

internal class ElasticsearchSink : ISink
{
    private readonly ElasticsearchSinkOptions _options;

    public ElasticsearchSink(ElasticsearchSinkOptions options)
    {
        _options = options;
    }

    public void Apply(LoggerConfiguration config)
    {
        // Elasticsearch sink konfigürasyonu
        config.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_options.NodeUri))
        {
            IndexFormat = _options.IndexFormat
        });
    }
}
```

```json
{
  "DotBoil": {
    "Logging": {
      "Sinks": [
        {
          "Key": "DotBoil:Logging:Elasticsearch",
          "NodeUri": "http://localhost:9200",
          "IndexFormat": "myapp-{0:yyyy.MM.dd}"
        }
      ]
    }
  }
}
```

---

## MediatR Pipeline Loglama

`DotBoil.Logging.Mediator` modülü eklendiğinde, tüm MediatR request/response'ları otomatik olarak loglanır:

```xml
<ProjectReference Include="..\DotBoil.Logging.Mediator\DotBoil.Logging.Mediator.csproj" />
```

```json
{
  "DotBoil": {
    "Mediator": {
      "Pipelines": [
        "DotBoil.Logging.LoggingBehaviour"
      ]
    }
  }
}
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `Serilog 4.3.0`
- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`