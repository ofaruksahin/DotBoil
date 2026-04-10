# DotBoil — Core Framework

DotBoil'in çekirdeğidir. Tüm modüller bu paketi referans alır. Modül yükleme altyapısını, merkezi konfigürasyon erişimini ve temel yetkilendirme filtrelerini içerir.

---

## Kurulum

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
await builder.AddDotBoil();   // Tüm modülleri keşfeder ve AddModule() çağırır

var app = builder.Build();
await app.UseDotBoil();       // Tüm modüllerin UseModule() metodunu çağırır

app.Run();
```

`HostApplicationBuilder` ile de kullanılabilir (Worker Service, Console App):

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.AddDotBoil();

var host = builder.Build();
await host.UseDotBoil();
await host.RunAsync();
```

---

## DotBoilApp

Uygulama genelinde merkezi erişim noktasıdır. Modüller bu static sınıf aracılığıyla konfigürasyona ve servis konteynerine erişir.

```csharp
// Konfigürasyon okuma
var options = DotBoilApp.Configuration.GetConfigurations<MyOptions>();

// Servis kaydetme (AddModule içinde)
DotBoilApp.Services.AddScoped<IMyService, MyService>();

// WebApplication erişimi (UseModule içinde)
var app = DotBoilApp.Host as WebApplication;
app?.UseMiddleware<MyMiddleware>();
```

| Üye | Tip | Açıklama |
|-----|-----|----------|
| `Configuration` | `IConfiguration` | appsettings.json ve environment variables |
| `Logging` | `ILoggingBuilder` | Log yapılandırması |
| `Services` | `IServiceCollection` | Dependency injection konteyneri |
| `Host` | `IHost` | Runtime host (WebApplication vb.) |

---

## Module Base Class

Yeni bir modül oluşturmak için `Module` abstract sınıfını kalıtın:

```csharp
internal class MyModule : Module
{
    public override string Name => "MyModule";

    // Bu modülün yüklenmesi için önce yüklenmesi gereken modüller
    public override IEnumerable<string> DependsOn =>
        new[] { "EFCore", "Caching" };

    // Küçük sayı = erken yükle. Bağımlılıkların Order değeri her zaman daha küçük olmalı.
    public override int Order => 5;

    public override Task AddModule()
    {
        // Servis kayıtları (builder.Build() öncesi)
        var opts = DotBoilApp.Configuration.GetConfigurations<MyOptions>();
        DotBoilApp.Services.AddScoped<IMyService, MyService>();
        DotBoilApp.Services.AddSingleton(opts);
        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        // Middleware ve runtime kurulum (app.Run() öncesi)
        var app = DotBoilApp.Host as WebApplication;
        app?.UseMiddleware<MyMiddleware>();
        return Task.CompletedTask;
    }
}
```

### Module Özellikleri

| Özellik | Açıklama |
|---------|----------|
| `Name` | Modülün benzersiz adı. `DependsOn` listesinde bu ad kullanılır. |
| `DependsOn` | Bu modül yüklenmeden önce yüklenmesi gereken modüllerin adları. |
| `Order` | Yükleme sırası. Küçük değer = önce yükle. |
| `AddModule()` | Servis kayıtları. `builder.Build()` çağrısından önce çalışır. |
| `UseModule()` | Middleware / endpoint kayıtları. `app.Run()` çağrısından önce çalışır. |

---

## IOptions — Konfigürasyon Pattern'i

Tüm modül konfigürasyonları `IOptions` interface'ini implement eder:

```csharp
internal class MyOptions : IOptions
{
    // appsettings.json'daki anahtar yolu
    public string Key => "DotBoil:MyModule";

    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
}
```

```json
{
  "DotBoil": {
    "MyModule": {
      "ConnectionString": "Server=localhost;...",
      "Timeout": 60
    }
  }
}
```

```csharp
// Okuma
var opts = DotBoilApp.Configuration.GetConfigurations<MyOptions>();
```

---

## Yetkilendirme Filtreleri

DotBoil iki temel yetkilendirme filtresi sunar:

### CheckRoleAuthorizationFilter

```csharp
// Endpoint'e uygulama
app.MapGet("/admin/users", GetUsers)
   .AddEndpointFilter<CheckRoleAuthorizationFilter>();
```

JWT token içindeki rol claim'lerini kontrol eder.

### CheckAppModuleAuthorizationFilter

```csharp
app.MapGet("/reports", GetReports)
   .AddEndpointFilter<CheckAppModuleAuthorizationFilter>();
```

Kullanıcının belirli bir uygulama modülüne erişim iznini kontrol eder.

---

## Temel Varlıklar

### BaseEntity

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Tüm EF Core entity'leri `BaseEntity`'den türemelidir.

---

## Konfigürasyon Anahtarı Standardı

```
DotBoil:{ModülAdı}:{Özellik}
```

Örnekler:
- `DotBoil:Caching:Password`
- `DotBoil:Logging:Sinks:0:Key`
- `DotBoil:EFCore:Contexts:0:ConnectionString`