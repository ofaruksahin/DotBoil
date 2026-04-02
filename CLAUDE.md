# Project: DotBoil

## 📖 Proje Açıklaması

**DotBoil**, .NET ekosisteminde sık yapılan işlemleri otomatize etmek ve modüler bir altyapı sağlamak amacıyla oluşturulmuş bir **boilerplate ve modüler framework**'tür. "DotNet" ve "Boilerplate" kavramlarından adını almıştır.

Proje, bağımsız modüllere dayalı bir mimari sunar. Her özellik bir modül olarak geliştirilir ve merkezi bir ClassLibrary (`DotBoil`) tarafından yönetilir. Bu yapı, kod tekrarını minimalize ederken bağımlılıkları kontrol altında tutar.

## 🏗️ Mimariye Genel Bakış

### Core Konsept

DotBoil, **modüler plugin sistem** olarak çalışır:

```
DotBoil (Core)
├── Modüllerin yüklenmesinden ve kullanılmasından sorumlu
├── Configuration yönetimi
├── Merkezi servis konteyner (DotBoilApp.Services)
└── Module base class tanımı

↓

Module Örnekleri
├── DotBoil.Caching
├── DotBoil.Logging
├── DotBoil.EFCore
├── DotBoil.Validator
├── ... (her modül DotBoil'i referans alır)
```

### Modül Bağımlılık Kuralları

**Temel Kural:** Hiçbir modül DotBoil dışında başka bir modülü direkt referans almaz.

**İstisnalar (Mediator Kombinasyonları):**
- `DotBoil.Logging.Mediator` → `DotBoil` + `DotBoil.Logging` + `MediatR` paketi
- `DotBoil.Validator.Mediator` → `DotBoil` + `DotBoil.Validator` + `MediatR` paketi

**Özel Durumlar:**
- `DotBoil.Localization` gibi modüller, bağımlılık problemi yaşamak istemediğimiz için doğrudan NuGet paketlerini kullanabilir (örn: `Microsoft.EntityFrameworkCore`, `StackExchange.Redis`)

### Ürün Projeler

DotBoil framework'ü içinde geliştirilen bağımsız ürünler:
- **DotBoil.AuthGuard.Application** - Yetkilendirme ve kimlik doğrulama
- **DotBoil.Studio.Core** - Dinamik form builder ve headless CMS

Bu projeler DotBoil'in diğer modüllerini referans alabilir.

## 📂 Dosya Yapısı ve Naming Convention

### Proje Yapısı

```
DotBoil/
├── DotBoil.csproj                    # Core framework
├── Exceptions/
├── Modules/
│   ├── Module.cs                     # Base class
│   └── IOptions.cs                   # Configuration interface
└── Extensions/

DotBoil.{ModuleName}/
├── DotBoil.{ModuleName}.csproj
├── {ModuleName}Module.cs             # ★ ZORUNLU - Module implementasyonu
├── Configuration/
│   ├── {ModuleName}Options.cs        # ★ Configuration class
│   └── Configure{ModuleName}Options.cs
├── Services/
├── Interfaces/
├── Entities/
└── Internal/                         # İç implementasyon

Örneğin DotBoil.Logging/:
├── LoggingModule.cs
├── Configuration/
│   ├── LoggingOptions.cs
│   └── ConfigureLoggingOptions.cs
├── Services/
│   └── LogService.cs
└── Internal/
```

### Naming Convention

| Eleman | Format | Örnek |
|--------|--------|-------|
| Proje Adı | `DotBoil.{ModuleName}` | `DotBoil.Caching` |
| Module Sınıfı | `{ModuleName}Module : Module` | `LoggingModule` |
| Options Sınıfı | `{ModuleName}Options : IOptions` | `LoggingOptions` |
| Configuration Sınıfı | `Configure{ModuleName}Options` | `ConfigureLoggingOptions` |
| Namespace | `DotBoil.{ModuleName}` | `namespace DotBoil.Logging;` |
| Klasörler | PascalCase | `Configuration/`, `Services/` |
| Dosya Adları | PascalCase | `LoggingModule.cs` |
| Sınıf/Interface Adları | PascalCase | `ILoggingService` |
| Private Metodlar | camelCase | `setupLogging()` |
| Private Alanlar | `_camelCase` | `_logger` |

## 🔧 Module Geliştirme Kuralları

### Module Base Class Yapısı

```csharp
internal class {ModuleName}Module : Module
{
    public override string Name => "Module Görünen Adı";
    
    public override IEnumerable<string> DependsOn { get; } 
        = Enumerable.Empty<string>();  // Diğer modüllere bağımlılık
    
    public override int Order { get; } = 0;  // Yükleme sırası
    
    // Modülü servis konteynerine ekle
    public override Task AddModule()
    {
        DotBoilApp.Services.Add...();
        return Task.CompletedTask;
    }
    
    // Middleware'leri ve runtime'ı konfigure et
    public override Task UseModule()
    {
        var app = DotBoilApp.Host as WebApplication;
        // app.Use...();
        return Task.CompletedTask;
    }
}
```

### Configuration Pattern

```csharp
// Configuration sınıfı (IOptions implement et)
internal class {ModuleName}Options : IOptions
{
    public string Key => "DotBoil:{ModuleName}";
    
    // Configuration özellikleri
    public string Property1 { get; set; }
    public int Property2 { get; set; }
}

// appsettings.json
{
  "DotBoil": {
    "{ModuleName}": {
      "Property1": "value",
      "Property2": 123
    }
  }
}

// Module'de kullanım
var options = DotBoilApp.Configuration
    .GetConfigurations<{ModuleName}Options>();
```

### Erişim Seviyeleri (Access Modifiers)

**Kural:** Olabildiğince `internal` kullan, sadece gerekli olanlar `public` olsun.

```csharp
// ✅ DOĞRU
internal class MyService { }           // Framework içi
internal interface IMyService { }      // Framework içi
public class PublicApi { }             // Uygulama tarafından kullanılacak
internal class Helper { }              // Sadece iç kullanım

// ❌ YANLIŞ
public class InternalHelper { }        // Gereksiz public
public interface IInternalService { }  // Gereksiz public
```

## 📦 Mevcut Modüller

### Core Modules

| Modül | Amaç | Bağımlılık |
|-------|------|-----------|
| `DotBoil` | Ana framework, modül yönetimi | - |
| `DotBoil.Caching` | Cache management (Memory, Distributed) | DotBoil |
| `DotBoil.Cors` | CORS configuration | DotBoil |
| `DotBoil.Cronos` | Scheduled jobs | DotBoil |
| `DotBoil.EFCore` | Entity Framework Core setup | DotBoil |
| `DotBoil.Email` | Email sending | DotBoil |
| `DotBoil.Health` | Health check endpoints | DotBoil |
| `DotBoil.Localization` | Multi-language support | DotBoil + EFCore, Redis (NuGet) |
| `DotBoil.Logging` | Logging infrastructure | DotBoil |
| `DotBoil.Mapper` | Object mapping (AutoMapper) | DotBoil |
| `DotBoil.MassTransit` | Message bus | DotBoil |
| `DotBoil.Mediator` | MediatR pipeline | DotBoil |
| `DotBoil.Parameter` | Application parameters | DotBoil |
| `DotBoil.Swag` | Swagger/OpenAPI | DotBoil |
| `DotBoil.TemplateEngine` | Razor view templates | DotBoil |
| `DotBoil.Validator` | FluentValidation | DotBoil |
| `DotBoil.Versioning` | API versioning | DotBoil |

### Mediator Kombinasyonları

| Modül | Bağımlılık | Amaç |
|-------|-----------|------|
| `DotBoil.Logging.Mediator` | DotBoil + Logging + MediatR | MediatR pipeline'da logging behavior |
| `DotBoil.Validator.Mediator` | DotBoil + Validator + MediatR | MediatR pipeline'da validation behavior |

### Ürün Projeler

| Proje | Amaç | Notlar |
|-------|------|--------|
| `DotBoil.AuthGuard.Application` | Yetkilendirme sistemi | Diğer modülleri referans alabilir |
| `DotBoil.Studio.Core` | Dinamik form builder CMS | Diğer modülleri referans alabilir |

## 🎯 Kodlama Kuralları

### Genel İlkeler

1. **Bağımlılık Minimizasyonu**
   - Modüller sadece DotBoil'i referans alır
   - Mediator kombinasyonları haricinde cross-module referans yok
   - NuGet paketleri direkt kullanılabilir

2. **Dosya Yapısını Basit Tut**
   - Gereksiz klasör yapısından kaçın
   - Logical grouping yap, dosya sayısı az ise root'ta tut
   - Configuration, Services, Internal klasörleri temel tutanlar

3. **Erişim Seviyeleri**
   - Default olarak `internal` kullan
   - Sadece uygulama geliştirme sırasında gerekli olan sınıflar `public`
   - Framework kullanıcılarına minimal API sunmayı amaçla

### Yükleme Sırası (Order)

Module'ün `Order` property'si yükleme sırasını belirler:

```csharp
// Yüksek priority modüller
public override int Order { get; } = 0;    // İlk yükle

// Orta priority
public override int Order { get; } = 5;

// Düşük priority (sonra yükle)
public override int Order { get; } = 10;
```

### Bağımlılık Tanımı (DependsOn)

```csharp
// Diğer modüllere bağlı ise
public override IEnumerable<string> DependsOn { get; } 
    = new[] { "Caching", "EFCore" };

// Bağımlılık yok ise
public override IEnumerable<string> DependsOn { get; } 
    = Enumerable.Empty<string>();
```

## 🔒 Güvenlik ve Best Practices

### Configuration Yönetimi

```csharp
// ✅ DOĞRU
var options = DotBoilApp.Configuration
    .GetConfigurations<MyOptions>();

// ❌ YANLIŞ
var value = Environment.GetEnvironmentVariable("KEY");
```

### Dependency Injection

```csharp
// Module'de servis kaydetme
DotBoilApp.Services.AddScoped<IMyService, MyService>();
DotBoilApp.Services.AddSingleton<ICacheService, CacheService>();

// Kullanıcı kodunda
var service = scope.ServiceProvider
    .GetRequiredService<IMyService>();
```

### Hata Yönetimi

- Module başlatma hatalarında açık exception fırlat
- Configuration eksikliği → açık hata mesajı
- Graceful fallback'ler ekle (mümkünse)

## 🚀 Yeni Modül Oluşturma Checklist

```
[ ] 1. Proje oluştur: DotBoil.{ModuleName}
[ ] 2. Module sınıfı: {ModuleName}Module.cs
    - Name, DependsOn, Order propertylerini doldur
    - AddModule() metodunu implement et
    - UseModule() metodunu implement et (WebApplication gerekiyorsa)
[ ] 3. Options sınıfı: Configuration/{ModuleName}Options.cs
    - IOptions interface'ini implement et
    - Key property'si: "DotBoil:{ModuleName}"
[ ] 4. Ana public API sınıfı (gerekiyorsa)
    - Uygulamacılar bunu kullanacak
[ ] 5. Internal implementasyon klasörü
    - Services, Interfaces, Entities vb.
[ ] 6. appsettings.json şeması dokumente et
[ ] 7. DotBoil.csproj'a modülü kaydet
[ ] 8. README.md ekle (konfigürasyon, örnek kullanım)
```

## 📝 Örnek: Yeni Bir Module Yazma

### Proje Dosyası (DotBoil.NewFeature.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DotBoil\DotBoil.csproj" />
  </ItemGroup>
</Project>
```

### Module Sınıfı

```csharp
namespace DotBoil.NewFeature;

internal class NewFeatureModule : Module
{
    public override string Name => "New Feature";
    public override IEnumerable<string> DependsOn { get; } 
        = Enumerable.Empty<string>();
    public override int Order { get; } = 0;

    public override Task AddModule()
    {
        var options = DotBoilApp.Configuration
            .GetConfigurations<NewFeatureOptions>();
        
        DotBoilApp.Services.AddScoped<INewFeatureService, NewFeatureService>();
        DotBoilApp.Services.AddSingleton(options);

        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        // WebApplication middleware'leri ekle (gerekiyorsa)
        return Task.CompletedTask;
    }
}
```

### Configuration

```csharp
namespace DotBoil.NewFeature.Configuration;

internal class NewFeatureOptions : IOptions
{
    public string Key => "DotBoil:NewFeature";
    
    public string Setting1 { get; set; }
    public int Setting2 { get; set; }
}
```

### appsettings.json

```json
{
  "DotBoil": {
    "NewFeature": {
      "Setting1": "value",
      "Setting2": 100
    }
  }
}
```

## 🚫 Kaçınılması Gereken Şeyler

| ❌ YANLIŞ | ✅ DOĞRU |
|----------|----------|
| Module arası direkt referans | Sadece DotBoil referansı |
| Hardcoded configuration | IOptions kullan |
| Public internal classes | internal erişim seviyesi |
| Stateful services | Stateless design |
| Module'de logic duplicate | Share through DotBoil base |
| Sıkı sıkıya bağlı modüller | Loosely coupled design |
| Deep folder hierarchy | Flat, simple structure |
| Configuration hardcode | appsettings.json + IOptions |

## 🔄 Git Workflow

### Branch Naming

```
feature/{ModuleName}/{feature-description}
fix/{module-or-area}/{bug-description}
docs/{what-is-documented}
```

### Commit Message Format

```
[MODULE_NAME] Brief description

Detailed explanation if needed.

Related: #issue-number
```

Örnek:
```
[Logging.Mediator] Add structured logging pipeline behavior

- Implement PipelineBehavior for request/response logging
- Add correlation ID tracking
- Configure log levels per module

Related: #123
```

## 📚 DotBoilApp Static Class

Merkezi application state ve configuration erişimi:

```csharp
// Configuration erişimi
DotBoilApp.Configuration.GetConfigurations<TOptions>()

// Servis kaydı
DotBoilApp.Services.AddScoped<T>()

// WebApplication'a erişim (runtime'da)
var app = DotBoilApp.Host as WebApplication

// Module yönetimi
DotBoilApp.UseModules()
```

## 🎓 Önemli Notlar

1. **Order ve DependsOn'ı Doğru Ayarla**
   - Eğer bir modül başka bir modülün servisi kullanıyorsa, DependsOn'da belirt
   - Order'ı uygun şekilde ayarla (dependencies'lerin Order'ı daha düşük olmalı)

2. **Configuration Key'i Standart Tut**
   - Hep `"DotBoil:{ModuleName}"` format'ında kullan

3. **AddModule vs UseModule**
   - `AddModule()`: Servis kaydı, konfigürasyon (Startup'ta)
   - `UseModule()`: Middleware'ler, runtime setup (Configure'da)

4. **Test Edilebilirlik**
   - Modülleri test edilebilir tutmaya çalış
   - Mock'lanabilir services design et

5. **Dokumentasyon**
   - Her modülün README'si olmalı
   - Configuration schema'sını dokumente et
   - Örnek kullanım ekle

## 📞 Claude Code Preferences

- Yeni modül oluştururken tüm yapıyı sorma
- Module dosyası değiştirilecekse önceden sor
- Cross-module referans eklemeden consultation iste
- Configuration schema değişiklikleri sor
- PublicAPI değişiklikleri sor
- DotBoil base class değişiklikleri sor

---

**Son Güncelleme:** Framework v1.x
**DotBoil Framework Kurucusu tarafından oluşturulmuştur**
