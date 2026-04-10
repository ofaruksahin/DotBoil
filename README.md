# DotBoil

DotBoil, .NET ekosisteminde her projede tekrar eden temel entegrasyonları otomatize etmek ve modüler bir altyapı sunmak amacıyla geliştirilmiş bir **boilerplate framework**'tür. "DotNet" ve "Boilerplate" kelimelerinden adını almıştır.

Proje, bağımsız modüllere dayalı bir mimari sunar. Her özellik ayrı bir modül olarak paketlenir; ihtiyaç duyduğunuz modülleri seçerek projenize eklersiniz. Böylece gereksiz bağımlılıklardan kaçınır, kod tekrarını minimuma indirirsiniz.

---

## Hızlı Başlangıç

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
await builder.AddDotBoil();

var app = builder.Build();
await app.UseDotBoil();

app.Run();
```

`AddDotBoil()` çağrısı, projenizin `src/` dizinindeki tüm modülleri otomatik olarak keşfeder ve yükler. Modülleri aktive etmek için yalnızca proje referansını eklemek yeterlidir.

---

## Mimari

```
DotBoil (Core)
├── Modül yükleme ve yaşam döngüsü yönetimi
├── Merkezi konfigürasyon erişimi (DotBoilApp.Configuration)
├── Merkezi servis konteyneri (DotBoilApp.Services)
└── Module abstract base class

Modüller
├── Her modül DotBoil'i referans alır
├── AddModule()  → Servis kayıtları (Startup)
├── UseModule()  → Middleware / Runtime kurulum
└── Order & DependsOn ile yükleme sırası belirlenir
```

### Modül Bağımlılık Kuralı

Hiçbir standart modül, DotBoil dışında başka bir modülü doğrudan referans almaz. **İstisnalar:**
- `DotBoil.Logging.Mediator` → Logging + Mediator kombinasyonu
- `DotBoil.Validator.Mediator` → Validator + Mediator kombinasyonu
- Ürün projeleri (`AuthGuard`, `Studio`) → İstedikleri modülleri referans alabilir

---

## Modüller

### Çekirdek

| Modül | Paket | Açıklama |
|-------|-------|----------|
| [DotBoil](src/DotBoil/README.md) | `DotBoil` | Framework çekirdeği; modül yükleme, konfigürasyon, yetkilendirme filtreleri |

### Standart Modüller

| Modül | Paket | Açıklama |
|-------|-------|----------|
| [Caching](src/DotBoil.Caching/README.md) | `DotBoil.Caching` | Redis tabanlı dağıtık önbellekleme |
| [Cors](src/DotBoil.Cors/README.md) | `DotBoil.Cors` | CORS politikası yapılandırması |
| [EFCore](src/DotBoil.EFCore/README.md) | `DotBoil.EFCore` | Entity Framework Core; repository pattern, interceptor, audit |
| [Email](src/DotBoil.Email/README.md) | `DotBoil.Email` | SMTP ile e-posta gönderimi (MailKit) |
| [Health](src/DotBoil.Health/README.md) | `DotBoil.Health` | Sağlık kontrol endpoint'leri ve UI |
| [Logging](src/DotBoil.Logging/README.md) | `DotBoil.Logging` | Serilog tabanlı yapılandırılabilir loglama |
| [Mapper](src/DotBoil.Mapper/README.md) | `DotBoil.Mapper` | AutoMapper profil otomatik keşfi |
| [MassTransit](src/DotBoil.MassTransit/README.md) | `DotBoil.MassTransit` | RabbitMQ mesaj kuyruğu; Inbox/Outbox pattern |
| [Mediator](src/DotBoil.Mediator/README.md) | `DotBoil.Mediator` | MediatR pipeline; handler otomatik keşfi |
| [Parameter](src/DotBoil.Parameter/README.md) | `DotBoil.Parameter` | Redis + MySQL tabanlı uygulama parametre yönetimi |
| [Localization](src/DotBoil.Localization/README.md) | `DotBoil.Localization` | Redis + MySQL tabanlı çoklu dil desteği |
| [Swag](src/DotBoil.Swag/README.md) | `DotBoil.Swag` | Swagger / OpenAPI dokümantasyon kurulumu |
| [TemplateEngine](src/DotBoil.TemplateEngine/README.md) | `DotBoil.TemplateEngine` | RazorLight ile Razor şablon motoru |
| [Validator](src/DotBoil.Validator/README.md) | `DotBoil.Validator` | FluentValidation; validator otomatik keşfi |
| [Versioning](src/DotBoil.Versioning/README.md) | `DotBoil.Versioning` | ASP.NET Core API versiyonlama |
| [Cronos](src/DotBoil.Cronos/README.md) | `DotBoil.Cronos` | Cron ifadesi tabanlı zamanlanmış görevler |
| [UserConsents](src/DotBoil.UserConsents/README.md) | `DotBoil.UserConsents` | Kullanıcı KVKK / onay yönetimi |

### Mediator Kombinasyonları

| Modül | Açıklama |
|-------|----------|
| [Logging.Mediator](src/DotBoil.Logging.Mediator/README.md) | MediatR pipeline'ında request/response loglama |
| [Validator.Mediator](src/DotBoil.Validator.Mediator/README.md) | MediatR pipeline'ında FluentValidation entegrasyonu |

### Ürün Projeleri

| Proje | Açıklama |
|-------|----------|
| [AuthGuard](src/DotBoil.AuthGuard.Application/README.md) | JWT tabanlı kimlik doğrulama ve yetkilendirme sistemi |
| [Studio](src/DotBoil.Studio.Core/README.md) | Dinamik form builder ve headless CMS (Blazor + MudBlazor) |

---

## Konfigürasyon Yapısı

Tüm modüller `appsettings.json` içindeki `DotBoil` anahtarı altında yapılandırılır:

```json
{
  "DotBoil": {
    "Caching": {
      "Endpoints": [{ "Ip": "127.0.0.1", "Port": 6379 }],
      "Password": ""
    },
    "Cors": {
      "PolicyName": "DefaultPolicy",
      "Origins": ["https://example.com"]
    },
    "Logging": {
      "Sinks": [
        { "Key": "DotBoil:Logging:Console" },
        { "Key": "DotBoil:Logging:File", "Path": "logs/app.log" }
      ]
    }
  }
}
```

---

## Yeni Modül Oluşturma

```
1. Proje oluştur: DotBoil.{ModuleName}
2. DotBoil.csproj'u referans ekle
3. {ModuleName}Module.cs → Module'ü inherit et
4. Configuration/{ModuleName}Options.cs → IOptions implement et
5. AddModule() içinde servisleri kaydet
6. UseModule() içinde middleware'leri ekle (gerekirse)
```

```csharp
internal class MyFeatureModule : Module
{
    public override string Name    => "MyFeature";
    public override int    Order   => 0;
    public override IEnumerable<string> DependsOn => Enumerable.Empty<string>();

    public override Task AddModule()
    {
        var options = DotBoilApp.Configuration.GetConfigurations<MyFeatureOptions>();
        DotBoilApp.Services.AddScoped<IMyFeatureService, MyFeatureService>();
        return Task.CompletedTask;
    }

    public override Task UseModule() => Task.CompletedTask;
}
```

---

## Teknoloji Yığını

| Teknoloji | Versiyon |
|-----------|---------|
| .NET | 10.0 |
| Entity Framework Core | 10.0.2 |
| MediatR | 14.0.0 |
| MassTransit + RabbitMQ | 8.5.8 |
| Serilog | 4.3.0 |
| AutoMapper | 16.0.0 |
| FluentValidation | 12.1.1 |
| Swashbuckle | 10.1.0 |
| MudBlazor | 9.0.0 |
| StackExchange.Redis | 2.10.1 |
| MailKit | 4.14.1 |
| RazorLight | 2.3.1 |
| Cronos | 0.7.2 |
