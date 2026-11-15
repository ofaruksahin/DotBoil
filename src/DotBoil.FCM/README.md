# DotBoil.FCM

Firebase Cloud Messaging (FCM) entegrasyonu. Login olmuş kullanıcıların cihazlarından gelen FCM registration token'larını veritabanında saklar ve **UserId** üzerinden push bildirim gönderir. Bir kullanıcının birden fazla cihazı (Android, iOS, Web) olabilir; bildirim kullanıcının tüm aktif cihazlarına gider.

---

## Kurulum

NuGet üzerinden:

```bash
dotnet add package DotBoil.FCM
```

veya proje referansı olarak:

```xml
<ProjectReference Include="..\DotBoil.FCM\DotBoil.FCM.csproj" />
```

Firebase Console → Project Settings → Service Accounts → **Generate new private key** ile service account JSON dosyasını indirin.

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "FCM": {
      "Firebase": {
        "CredentialPath": "/secrets/firebase-service-account.json"
      },
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=MyApp"
      }
    }
  }
}
```

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `Firebase:CredentialPath` | `CredentialJson` yoksa | Service account JSON dosyasının yolu |
| `Firebase:CredentialJson` | `CredentialPath` yoksa | Service account JSON içeriği (secret store / environment üzerinden vermek için). Doluysa `CredentialPath`'e göre önceliklidir |
| `Persistence:ConnectionString` | Evet | MySQL bağlantı cümlesi |

Eksik konfigürasyonda modül başlatılırken açık bir hata mesajıyla exception fırlatılır.

Modül kendi Firebase instance'ını `DotBoil.FCM` adıyla oluşturur; host uygulamanın kullandığı default `FirebaseApp` ile çakışmaz.

---

## Kullanım

```csharp
public class OrderNotificationService
{
    private readonly IFCMService _fcmService;

    public OrderNotificationService(IFCMService fcmService)
    {
        _fcmService = fcmService;
    }

    public async Task NotifyOrderShippedAsync(string userId, int orderId)
    {
        var result = await _fcmService.SendToUser(userId, new FCMNotification
        {
            Title = "Siparişiniz kargoya verildi",
            Body = $"#{orderId} numaralı siparişiniz yola çıktı.",
            Data = new Dictionary<string, string>
            {
                ["type"] = "order_shipped",
                ["orderId"] = orderId.ToString()
            }
        });

        // result.SuccessCount, result.FailureCount, result.RemovedTokenCount
    }

    // Kullanıcı hesabını sildiğinde tüm cihaz kayıtlarını pasifle
    public Task OnUserDeletedAsync(string userId)
        => _fcmService.UnregisterUser(userId);
}
```

`Title` ve `Body` boş bırakılırsa yalnızca `Data` içeren **data-only (sessiz)** mesaj gönderilir.

---

## Job / Worker Projesinde Kullanım

Modül API dışındaki projelerde de (Worker Service, Console, Cronos job vb.) kullanılabilir. Token'lar API tarafında kaydedilir; job projesi aynı veritabanını ve Firebase service account'u kullanarak doğrudan gönderim yapar.

```xml
<ProjectReference Include="..\DotBoil.FCM\DotBoil.FCM.csproj" />
```

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.AddDotBoil();

var host = builder.Build();
await host.UseDotBoil();   // Migration uygulanır; WebApplication olmadığı için endpoint eklenmez

await host.RunAsync();
```

Job projesinin `appsettings.json` dosyasında API ile **aynı** `DotBoil:FCM` konfigürasyonu bulunmalıdır.

```csharp
public class CampaignNotificationJob
{
    private readonly FCMService _fcmService;   // veya IFCMService

    public CampaignNotificationJob(FCMService fcmService)
    {
        _fcmService = fcmService;
    }

    public async Task ExecuteAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var result = await _fcmService.SendToUsers(userIds, new FCMNotification
        {
            Title = "Kampanya başladı",
            Body = "Size özel indirimleri kaçırmayın!"
        }, ct);
    }
}
```

`FCMService` public'tir ancak constructor'ı internal'dır; instance her zaman DI üzerinden alınmalıdır (`new` ile oluşturulamaz). `FCMService` ve `IFCMService` Scoped kayıtlıdır; Singleton bir servis veya `BackgroundService` içinden kullanılacaksa scope açılmalıdır:

```csharp
using var scope = serviceProvider.CreateScope();
var fcmService = scope.ServiceProvider.GetRequiredService<FCMService>();
```

---

## IFCMService Arayüzü

```csharp
public interface IFCMService
{
    // Kullanıcının cihaz token'ını kaydeder / günceller
    Task RegisterToken(string userId, string token, DevicePlatform platform, string deviceId = null, CancellationToken ct = default);

    // Token'ı pasifler (logout)
    Task UnregisterToken(string token, CancellationToken ct = default);

    // Kullanıcının tüm token'larını pasifler
    Task UnregisterUser(string userId, CancellationToken ct = default);

    // Kullanıcının tüm aktif cihazlarına bildirim gönderir
    Task<FCMSendResult> SendToUser(string userId, FCMNotification notification, CancellationToken ct = default);

    // Birden fazla kullanıcıya bildirim gönderir
    Task<FCMSendResult> SendToUsers(IEnumerable<string> userIds, FCMNotification notification, CancellationToken ct = default);
}
```

### Modeller

```csharp
public enum DevicePlatform { Android = 1, iOS = 2, Web = 3 }

public sealed class FCMNotification
{
    public string Title { get; init; }
    public string Body { get; init; }
    public string ImageUrl { get; init; }
    public IReadOnlyDictionary<string, string> Data { get; init; }
}

public sealed class FCMSendResult
{
    public int TokenCount { get; init; }         // Gönderim yapılan token sayısı
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public int RemovedTokenCount { get; init; }  // Geçersiz olduğu için pasiflenen token sayısı
}
```

---

## Token Yaşam Döngüsü

- **Token tekildir.** Aynı token farklı bir kullanıcıyla tekrar kaydedilirse (aynı cihazda başka hesapla login) kayıt yeni kullanıcıya devredilir; eski kullanıcı o cihaza bildirim almaz.
- **DeviceId** gönderilirse, aynı kullanıcı + aynı cihaz için önceki token'lar otomatik pasiflenir (Firebase token'ı yenilediğinde eski token birikmez).
- Gönderim sırasında FCM `Unregistered` veya `SenderIdMismatch` hatası dönen token'lar otomatik pasiflenir.
- Silme işlemleri soft delete'tir (`IsDeleted`); aynı token tekrar kaydedilirse kayıt yeniden aktifleşir.
- Gönderim 500'lük paketler halinde yapılır (FCM multicast limiti).

İstemci tarafında önerilen akış: login sonrası ve her `onTokenRefresh` olayında `POST /api/fcm/tokens`, logout'ta `POST /api/fcm/tokens/unregister`.

---

## Veritabanı

Modül `UseDotBoil()` sırasında kendi migration'larını otomatik uygular ve `UserDeviceTokens` tablosunu oluşturur.

| Kolon | Açıklama |
|-------|----------|
| `UserId` | Kullanıcı kimliği (max 100) |
| `Token` | FCM registration token (max 512, unique index) |
| `Platform` | `DevicePlatform` (1: Android, 2: iOS, 3: Web) |
| `DeviceId` | Opsiyonel cihaz kimliği (max 200) |
| `LastSeenTime` | Token'ın en son kaydedildiği / yenilendiği zaman |
| `IsDeleted` | Soft delete bayrağı (`UserId` + `IsDeleted` index'li) |

---

## API Endpoint'leri

Modül yüklendiğinde aşağıdaki endpoint'ler otomatik eklenir:

| Method | Path | Açıklama |
|--------|------|----------|
| `POST` | `/api/fcm/tokens` | Login olmuş kullanıcının cihaz token'ını kaydet |
| `POST` | `/api/fcm/tokens/unregister` | Kullanıcının kendi token'ını pasifle (logout) |
| `POST` | `/api/fcm/send` | Belirtilen kullanıcılara bildirim gönder (Admin) |

Kullanıcı kimliği `ClaimTypes.NameIdentifier` claim'inden okunur. Kullanıcı yalnızca kendi token'ını pasifleyebilir.

### Token kaydı

```http
POST /api/fcm/tokens
Content-Type: application/json

{ "token": "fcm-registration-token", "platform": 1, "deviceId": "optional-device-id" }
```

### Logout

```http
POST /api/fcm/tokens/unregister
Content-Type: application/json

{ "token": "fcm-registration-token" }
```

### Bildirim gönderimi (Admin)

```http
POST /api/fcm/send
Content-Type: application/json

{
  "userIds": ["42", "43"],
  "title": "Merhaba",
  "body": "Yeni bir mesajınız var",
  "imageUrl": null,
  "data": { "type": "message", "messageId": "123" }
}
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `FCMService` | — | Scoped |
| `IFCMService` | `FCMService` | Scoped |
| `FCMDbContext` | — | Scoped |
| `FirebaseMessaging` | `DotBoil.FCM` isimli FirebaseApp | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `FirebaseAdmin`
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`
