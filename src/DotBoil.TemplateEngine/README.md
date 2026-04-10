# DotBoil.TemplateEngine

RazorLight kullanarak Razor sözdiziminde e-posta ve bildirim şablonları oluşturur. `IRazorRenderer` arayüzü ile şablonları model bağlayarak HTML'e render eder.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.TemplateEngine\DotBoil.TemplateEngine.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "RazorViewEngine": {
      "AssemblyName": "MyApp.Templates",
      "RootNamespace": "MyApp.Templates.Views"
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `AssemblyName` | `string` | Şablonları içeren assembly'nin adı |
| `RootNamespace` | `string` | Şablon dosyalarının kök namespace'i |

---

## Şablon Oluşturma

Şablon dosyaları `.cshtml` uzantılı olup assembly embedded resource olarak işaretlenmelidir:

```xml
<!-- MyApp.Templates.csproj -->
<ItemGroup>
  <EmbeddedResource Include="Views/**/*.cshtml" />
</ItemGroup>
```

### Örnek: Hoşgeldin E-postası

`Views/Emails/WelcomeEmail.cshtml`:

```cshtml
@model WelcomeEmailModel
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Hoş Geldiniz</title>
</head>
<body>
    <h1>Merhaba @Model.UserName!</h1>
    <p>Hesabınız başarıyla oluşturuldu.</p>
    <p>
        <a href="@Model.ActivationLink">Hesabınızı aktive etmek için tıklayın</a>
    </p>
    <p>Bu bağlantı @Model.ExpiresInHours saat geçerlidir.</p>
</body>
</html>
```

Model sınıfı:

```csharp
public class WelcomeEmailModel
{
    public string UserName        { get; set; } = string.Empty;
    public string ActivationLink  { get; set; } = string.Empty;
    public int ExpiresInHours     { get; set; } = 24;
}
```

---

## Kullanım

```csharp
public class EmailNotificationService
{
    private readonly IRazorRenderer _renderer;
    private readonly IMailSender    _mailSender;

    public EmailNotificationService(IRazorRenderer renderer, IMailSender mailSender)
    {
        _renderer   = renderer;
        _mailSender = mailSender;
    }

    public async Task SendWelcomeAsync(string toEmail, string userName, string token)
    {
        var htmlBody = await _renderer.RenderAsync("Emails/WelcomeEmail", new WelcomeEmailModel
        {
            UserName       = userName,
            ActivationLink = $"https://example.com/activate?token={token}",
            ExpiresInHours = 24
        });

        await _mailSender.SendAsync(serverSettings, new Message(
            from:        new List<string> { "noreply@example.com" },
            to:          new List<string> { toEmail },
            subject:     $"Hoş Geldiniz, {userName}!",
            body:        htmlBody,
            attachments: new List<Attachment>()
        ));
    }
}
```

---

## IRazorRenderer Arayüzü

```csharp
public interface IRazorRenderer
{
    // Model olmadan render (static şablonlar)
    Task<string> RenderAsync(string templateName);

    // Model ile render
    Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class;
}
```

---

## Şablon Adı Kuralı

`templateName` parametresi assembly'deki kaynak yoluna karşılık gelir:

| Dosya Konumu | templateName Değeri |
|--------------|---------------------|
| `Views/Emails/WelcomeEmail.cshtml` | `"Emails/WelcomeEmail"` |
| `Views/Notifications/PushNotification.cshtml` | `"Notifications/PushNotification"` |
| `Views/Invoice.cshtml` | `"Invoice"` |

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `IRazorRenderer` | `RazorRenderer` | Singleton |
| `RazorLightEngine` | — | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `RazorLight 2.3.1`