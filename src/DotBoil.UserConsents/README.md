# DotBoil.UserConsents

Kullanıcı KVKK / onay yönetimi. Farklı türlerdeki onay metinlerini (KVKK, Gizlilik Politikası, Kullanım Şartları vb.) veritabanında versiyonlayarak saklar; kullanıcıların hangi versiyonu onayladığını takip eder.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.UserConsents\DotBoil.UserConsents.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "UserConsents": {
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=MyApp"
      }
    }
  }
}
```

---

## Kullanım

```csharp
public class RegistrationService
{
    private readonly IUserConsentsService _consentsService;

    public RegistrationService(IUserConsentsService consentsService)
    {
        _consentsService = consentsService;
    }

    // Kullanıcıya gösterilecek onay metnini getir
    public async Task<ConsentResult> GetKvkkTextAsync(string language)
    {
        return await _consentsService.GetConsent(ConsentType.Kvkk, language);
    }

    // Tekil onay kaydet
    public async Task AcceptKvkkAsync(string userId, string language)
    {
        await _consentsService.AcceptConsent(userId, ConsentType.Kvkk, language);
    }

    // Kayıt sırasında tüm zorunlu onayları tek seferde kaydet
    public async Task AcceptAllConsentsAsync(string userId, string language)
    {
        await _consentsService.AcceptConsents(
            userId,
            new[] { ConsentType.Kvkk, ConsentType.TermsOfService, ConsentType.PrivacyPolicy },
            language
        );
    }
}
```

---

## IUserConsentsService Arayüzü

```csharp
public interface IUserConsentsService
{
    // Aktif onay metnini ve meta bilgilerini getirir
    Task<ConsentResult> GetConsent(ConsentType type, string language, CancellationToken ct = default);

    // Kullanıcının onayladığını kaydeder
    Task AcceptConsent(string userId, ConsentType type, string language, CancellationToken ct = default);

    // Birden fazla onayı tek seferde kaydeder
    Task AcceptConsents(string userId, IEnumerable<ConsentType> types, string language, CancellationToken ct = default);

    // Aynı işlemler; kabul anındaki istemci IP adresi ve User-Agent bilgisiyle birlikte kaydeder
    Task AcceptConsent(string userId, ConsentType type, string language, string ipAddress, string userAgent, CancellationToken ct = default);
    Task AcceptConsents(string userId, IEnumerable<ConsentType> types, string language, string ipAddress, string userAgent, CancellationToken ct = default);
}
```

IP adresi ve User-Agent içermeyen overload'lar bu alanları `null` olarak kaydeder. User-Agent 512, IP adresi 45 karakterden uzunsa kırpılır.

### ConsentResult

```csharp
public sealed class ConsentResult
{
    public int         Id         { get; init; }
    public ConsentType Type       { get; init; }
    public string      Language   { get; init; }
    public string      Content    { get; init; }    // HTML onay metni
    public bool        IsRequired { get; init; }    // Zorunlu onay mı?
    public int         Version    { get; init; }    // Mevcut versiyon numarası
}
```

---

## API Endpoint'leri

Modül yüklendiğinde aşağıdaki endpoint'ler otomatik eklenir:

| Method | Path | Açıklama |
|--------|------|----------|
| `GET` | `/api/consents` | Tüm onay metinlerini listele (Admin) |
| `GET` | `/api/consents/{id}` | Onay metnini getir (Admin) |
| `POST` | `/api/consents` | Yeni onay metni ekle (Admin) |
| `PUT` | `/api/consents/{id}` | Onay metnini güncelle, versiyonu artır (Admin) |
| `DELETE` | `/api/consents/{id}` | Onay metnini sil (Admin) |
| `GET` | `/api/consents/check?type=..&language=..` | Kullanıcının tek bir onayı güncel mi |
| `POST` | `/api/consents/accept` | Tek bir onayı kabul et |
| `GET` | `/api/consents/check/bulk?types=..&types=..&language=..` | Kullanıcının birden fazla onayını tek seferde kontrol et |
| `POST` | `/api/consents/accept/bulk` | Birden fazla onayı tek seferde kabul et |

`accept` ve `accept/bulk` endpoint'leri kabul kaydıyla birlikte isteğin **IP adresini** (`Connection.RemoteIpAddress`) ve **User-Agent** başlığını da saklar. Uygulama bir reverse proxy / load balancer arkasındaysa gerçek istemci IP'sinin kaydedilmesi için host uygulamada `UseForwardedHeaders()` yapılandırılmalıdır.

### Toplu kontrol

```http
GET /api/consents/check/bulk?types=TermsOfService&types=PrivacyPolicy&language=tr
```

```json
{
  "isAllUpToDate": false,
  "isAllRequiredUpToDate": true,
  "consents": [
    { "type": 1, "exists": true, "isRequired": true,  "isUpToDate": true,  "currentVersion": 2, "acceptedVersion": 2, "content": "..." },
    { "type": 2, "exists": true, "isRequired": false, "isUpToDate": false, "currentVersion": 3, "acceptedVersion": 1, "content": "..." }
  ]
}
```

### Toplu kabul

```http
POST /api/consents/accept/bulk
Content-Type: application/json

{ "types": [1, 2, 3], "language": "tr" }
```

Listedeki tiplerden herhangi biri ilgili dilde bulunamazsa hiçbir kayıt yapılmaz ve `404` döner.

---

## Onay Versiyonlama

Onay metni her güncellendiğinde yeni bir versiyon oluşturulur. Eski versiyonu onaylamış kullanıcılar için yeniden onay talep edilebilir:

```csharp
// Kullanıcının onayladığı versiyon ile güncel versiyon kontrolü
var currentConsent = await _consentsService.GetConsent(ConsentType.Kvkk, "TR");

// Mevcut versiyon kullanıcının onayladığı versiyondan yeniyse yeniden onay iste
if (currentConsent.Version > user.AcceptedKvkkVersion)
{
    // Kullanıcıyı yeniden onay sayfasına yönlendir
}
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `IUserConsentsService` | `UserConsentsService` | Scoped |
| `UserConsentsDbContext` | — | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Microsoft.EntityFrameworkCore`
- `MySql.EntityFrameworkCore`