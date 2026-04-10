# DotBoil.Email

MailKit kullanarak SMTP üzerinden e-posta gönderimi sağlar. `IMailSender` arayüzü ile düz metin veya HTML içerikli e-postalar, ekler ile birlikte gönderilebilir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Email\DotBoil.Email.csproj" />
```

---

## Konfigürasyon

SMTP ayarları kod tarafından `ServerSettings` nesnesi olarak `SendAsync` metoduna geçirilir; appsettings.json'dan okunması tavsiye edilir:

```json
{
  "Mail": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "noreply@example.com",
    "Password": "smtp-password",
    "UseSsl": true
  }
}
```

---

## Kullanım

```csharp
public class NotificationService
{
    private readonly IMailSender _mailSender;
    private readonly IConfiguration _config;

    public NotificationService(IMailSender mailSender, IConfiguration config)
    {
        _mailSender = mailSender;
        _config = config;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var serverSettings = new ServerSettings
        {
            Host     = _config["Mail:Host"],
            Port     = int.Parse(_config["Mail:Port"]),
            Username = _config["Mail:Username"],
            Password = _config["Mail:Password"],
            UseSsl   = bool.Parse(_config["Mail:UseSsl"])
        };

        var message = new Message(
            from:        new List<string> { "noreply@example.com" },
            to:          new List<string> { toEmail },
            subject:     "Hoş Geldiniz!",
            body:        $"<h1>Merhaba {userName}</h1><p>Hesabınız oluşturuldu.</p>",
            attachments: new List<Attachment>()
        );

        var success = await _mailSender.SendAsync(serverSettings, message);

        if (!success)
            throw new Exception("E-posta gönderilemedi.");
    }
}
```

---

## IMailSender Arayüzü

```csharp
public interface IMailSender
{
    Task<bool> SendAsync(ServerSettings settings, Message message);
}
```

### Message Modeli

```csharp
public class Message
{
    public List<string> From        { get; set; }
    public List<string> To          { get; set; }
    public string       Subject     { get; set; }
    public string       Body        { get; set; }   // HTML destekler
    public List<Attachment> Attachments { get; set; }
}
```

---

## Ek (Attachment) ile Gönderim

```csharp
var attachment = new Attachment
{
    FileName    = "rapor.pdf",
    ContentType = "application/pdf",
    Data        = File.ReadAllBytes("/path/to/rapor.pdf")
};

var message = new Message(
    from:        new List<string> { "sender@example.com" },
    to:          new List<string> { "recipient@example.com" },
    subject:     "Aylık Rapor",
    body:        "<p>Raporunuz ekte yer almaktadır.</p>",
    attachments: new List<Attachment> { attachment }
);

await _mailSender.SendAsync(serverSettings, message);
```

---

## TemplateEngine ile Birlikte Kullanım

E-posta gövdesi için HTML şablonları `DotBoil.TemplateEngine` modülü ile oluşturulabilir:

```csharp
// Razor şablonunu render et
var htmlBody = await _razorRenderer.RenderAsync("WelcomeEmail", new
{
    UserName   = "Ali Veli",
    ActivationLink = "https://example.com/activate?token=abc"
});

var message = new Message(
    from:        new List<string> { "noreply@example.com" },
    to:          new List<string> { "ali@example.com" },
    subject:     "Hesabınızı Aktive Edin",
    body:        htmlBody,
    attachments: new List<Attachment>()
);

await _mailSender.SendAsync(serverSettings, message);
```

---

## Kayıtlı Servisler

| Servis | Uygulama | Yaşam Süresi |
|--------|----------|--------------|
| `IMailSender` | `SmtpSender` | Singleton |

---

## Bağımlılıklar

- `DotBoil` (core)
- `MailKit 4.14.1`