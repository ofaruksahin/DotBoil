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
}
```

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
| `GET` | `/consents/{type}/{language}` | Onay metnini getir |
| `POST` | `/consents/accept` | Kullanıcı onayını kaydet |
| `POST` | `/consents/accept-all` | Birden fazla onayı kaydet |
| `GET` | `/consents/admin` | Tüm onayları listele (admin) |
| `POST` | `/consents/admin` | Yeni onay versiyonu ekle (admin) |

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