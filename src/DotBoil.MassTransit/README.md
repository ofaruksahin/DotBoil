# DotBoil.MassTransit

RabbitMQ mesaj kuyruğu entegrasyonu. Inbox/Outbox pattern ile mesaj güvenilirliği, Consumer otomatik keşfi ve `IBusPublisher` arayüzü ile kolay mesaj yayımlama sağlar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.MassTransit\DotBoil.MassTransit.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "MessageBroker": {
      "MassTransit": {
        "Persistence": {
          "PersistenceType": "MySql",
          "MySql": {
            "ConnectionString": "server=localhost;uid=root;pwd=pass;database=MyApp"
          }
        },
        "RabbitMq": {
          "Host": "localhost",
          "Port": 5672,
          "Username": "guest",
          "Password": "guest",
          "RetryCount": 3,
          "RetryInterval": 5
        }
      }
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Persistence.PersistenceType` | `string` | `MySql` — Inbox/Outbox storage tipi |
| `Persistence.MySql.ConnectionString` | `string` | MySQL bağlantı dizesi |
| `RabbitMq.Host` | `string` | RabbitMQ sunucu adresi |
| `RabbitMq.Port` | `int` | RabbitMQ portu (varsayılan: 5672) |
| `RabbitMq.Username` | `string` | RabbitMQ kullanıcı adı |
| `RabbitMq.Password` | `string` | RabbitMQ şifresi |
| `RabbitMq.RetryCount` | `int` | Yeniden deneme sayısı |
| `RabbitMq.RetryInterval` | `int` | Yeniden denemeler arası bekleme (saniye) |

---

## Event Tanımlama

Olaylar `IEvent` arayüzünü implement eder:

```csharp
public class OrderCreatedEvent : IEvent
{
    public int OrderId     { get; set; }
    public string UserId   { get; set; } = string.Empty;
    public decimal Amount  { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## Mesaj Yayımlama (Publisher)

```csharp
public class OrderService
{
    private readonly IBusPublisher _busPublisher;

    public OrderService(IBusPublisher busPublisher)
    {
        _busPublisher = busPublisher;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        // Sipariş oluşturma...
        var order = new Order { /* ... */ };

        // Olay yayımla
        await _busPublisher.Publish(new OrderCreatedEvent
        {
            OrderId   = order.Id,
            UserId    = request.UserId,
            Amount    = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        });
    }
}
```

---

## Consumer Tanımlama

`IConsumer<T>` ve `[Consumer]` attribute'ü ile consumer'ları tanımlayın:

```csharp
[Consumer("order-created-notification")]
public class OrderCreatedNotificationConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IMailSender _mailSender;

    public OrderCreatedNotificationConsumer(IMailSender mailSender)
    {
        _mailSender = mailSender;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var order = context.Message;

        await _mailSender.SendAsync(serverSettings, new Message(
            from:    new List<string> { "noreply@example.com" },
            to:      new List<string> { $"{order.UserId}@example.com" },
            subject: $"Siparişiniz Alındı - #{order.OrderId}",
            body:    $"<p>Siparişiniz alındı. Tutar: {order.Amount:C}</p>",
            attachments: new List<Attachment>()
        ));
    }
}
```

`[Consumer("queue-name")]` attribute'ündeki isim, RabbitMQ'da oluşturulacak kuyruğun adıdır.

---

## IBusPublisher Arayüzü

```csharp
public interface IBusPublisher
{
    Task Publish<T>(T message) where T : IEvent;
}
```

---

## Inbox / Outbox Pattern

Mesaj kayıplarını önlemek için Inbox/Outbox pattern otomatik olarak aktif edilir:

- **Outbox**: Mesajlar önce veritabanına kaydedilir, sonra RabbitMQ'ya gönderilir. Uygulama çökse bile mesaj kaybolmaz.
- **Inbox**: Aynı mesajın birden fazla işlenmesini engeller (idempotency).

Outbox için DbContext'e interceptor ekleyin:

```json
{
  "DotBoil": {
    "EFCore": {
      "Contexts": [
        {
          "TypeName": "MyApp.Infrastructure.AppDbContext",
          "Interceptors": [
            "DotBoil.MassTransit.Persistence.MassTransitDbContextSaveChangesInterceptor"
          ]
        }
      ]
    }
  }
}
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `IBusPublisher` | `RabbitMqPublisher` | Scoped |
| MassTransit Bus | — | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `MassTransit 8.5.8`
- `MassTransit.RabbitMQ 8.5.8`
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`