# DotBoil.AuthGuard

JWT tabanlı kimlik doğrulama ve yetkilendirme sistemi. Kullanıcı yönetimi, rol tabanlı erişim kontrolü, menü yetkilendirmesi, OAuth 2.0 harici giriş ve çok kiracılı (multi-tenant) mimariyi destekler.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.AuthGuard.Application\DotBoil.AuthGuard.Application.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "AuthGuard": {
      "DbContext": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=AuthGuard"
      },
      "JwtOptions": {
        "SecretKey": "your-very-long-secret-key-min-32-chars",
        "Issuer": "https://auth.example.com",
        "Audience": "https://app.example.com",
        "AccessTokenExpirationMinutes": 60,
        "RefreshTokenExpirationMinutes": 10080
      }
    },
    "Localization": {
      "Caching": {
        "ConnectionString": "127.0.0.1:6379",
        "ExpireInHour": 12
      },
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=AuthGuard"
      }
    },
    "Parameters": {
      "Caching": {
        "ConnectionString": "127.0.0.1:6379",
        "ExpireInHour": 12
      },
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=AuthGuard"
      }
    },
    "Caching": {
      "Redis": {
        "Endpoints": [{ "IpAddress": "127.0.0.1", "Port": 6379 }],
        "Password": ""
      }
    },
    "Logging": {
      "FileSink": {
        "LogFileName": "logs/authguard",
        "RollingInterval": "Day"
      }
    },
    "Mediator": {
      "Pipelines": [
        "DotBoil.Validator.ValidationBehaviour"
      ]
    },
    "EFCore": {
      "Contexts": [
        {
          "TypeName": "DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts.DotBoilAuthGuardDbContext",
          "Interceptors": [
            "DotBoil.EFCore.Interceptors.AuditInterceptor",
            "DotBoil.MassTransit.Persistence.MassTransitDbContextSaveChangesInterceptor"
          ]
        }
      ]
    },
    "Cors": {
      "PolicyName": "Default",
      "Origins": ["https://app.example.com"]
    },
    "AvailableLanguage": [
      { "Shortcut": "TR", "Name": "Türkçe", "Icon": "flag-icon-tr" },
      { "Shortcut": "EN", "Name": "English", "Icon": "flag-icon-us" }
    ],
    "MessageBroker": {
      "MassTransit": {
        "Persistence": {
          "PersistenceType": "MySql",
          "MySql": {
            "ConnectionString": "server=localhost;uid=root;pwd=pass;database=AuthGuard"
          }
        },
        "RabbitMq": {
          "Host": "localhost",
          "Port": 5672,
          "Username": "guest",
          "Password": "guest"
        }
      }
    }
  }
}
```

---

## Harici Giriş (OAuth 2.0)

```json
{
  "DotBoil": {
    "ExternalSignInManagers": [
      {
        "Logo": "fa fa-github",
        "ServiceName": "Github",
        "AuthorizationEndpoint": "https://github.com/login/oauth/authorize",
        "TokenEndpoint": "https://github.com/login/oauth/access_token",
        "UserInfoEndpoint": "https://api.github.com/user",
        "EmailIdentifier": "email",
        "UsernameIdentifier": "login",
        "NameIdentifier": "name",
        "SurnameIdentifier": "",
        "RedirectUrl": "https://app.example.com/auth/callback"
      },
      {
        "Logo": "fa fa-google",
        "ServiceName": "Google",
        "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
        "TokenEndpoint": "https://oauth2.googleapis.com/token",
        "UserInfoEndpoint": "https://www.googleapis.com/oauth2/v3/userinfo",
        "EmailIdentifier": "email",
        "UsernameIdentifier": "email",
        "NameIdentifier": "given_name",
        "SurnameIdentifier": "family_name",
        "RedirectUrl": "https://app.example.com/auth/callback"
      }
    ]
  }
}
```

---

## API Endpoint'leri

### Kimlik Doğrulama

| Method | Path | Açıklama |
|--------|------|----------|
| `POST` | `/auth/login` | Kullanıcı adı/şifre ile giriş |
| `POST` | `/auth/refresh` | Access token yenileme |
| `POST` | `/auth/logout` | Oturumu kapat |
| `GET` | `/auth/external/{provider}` | Harici sağlayıcı giriş yönlendirmesi |
| `GET` | `/auth/external/{provider}/callback` | OAuth callback |

### Kullanıcı Yönetimi

| Method | Path | Açıklama |
|--------|------|----------|
| `GET` | `/users` | Kullanıcı listesi |
| `GET` | `/users/{id}` | Kullanıcı detayı |
| `POST` | `/users` | Kullanıcı oluştur |
| `PUT` | `/users/{id}` | Kullanıcı güncelle |
| `DELETE` | `/users/{id}` | Kullanıcı sil |
| `POST` | `/users/{id}/roles` | Role ata |

### Rol Yönetimi

| Method | Path | Açıklama |
|--------|------|----------|
| `GET` | `/roles` | Rol listesi |
| `POST` | `/roles` | Rol oluştur |
| `PUT` | `/roles/{id}` | Rol güncelle |
| `DELETE` | `/roles/{id}` | Rol sil |
| `POST` | `/roles/{id}/menus` | Role menü ata |
| `POST` | `/roles/{id}/modules` | Role modül ata |

### Menü Yönetimi

| Method | Path | Açıklama |
|--------|------|----------|
| `POST` | `/menus` | Kullanıcının yetkili menülerini getir |
| `GET` | `/menus/all` | Tüm menüleri listele |
| `POST` | `/menus/create` | Menü oluştur |
| `PUT` | `/menus/{id}` | Menü güncelle |
| `DELETE` | `/menus/{id}` | Menü sil |

---

## JWT Token Yapısı

Token içindeki standart claim'ler:

| Claim | Açıklama |
|-------|----------|
| `sub` | Kullanıcı ID'si |
| `name` | Ad |
| `family_name` | Soyad |
| `email` | E-posta |
| `preferred_username` | Kullanıcı adı |
| `role` | Roller listesi |
| `tenant_id` | Kiracı ID'si |
| `exp` | Token geçerlilik süresi |

---

## Yetkilendirme Filtresi Kullanımı

```csharp
// Rol kontrolü
app.MapGet("/admin/users", GetUsers)
   .AddEndpointFilter<CheckRoleAuthorizationFilter>();

// Uygulama modülü kontrolü
app.MapGet("/reports", GetReports)
   .AddEndpointFilter<CheckAppModuleAuthorizationFilter>();
```

---

## Migration

```bash
dotnet ef database update --project DotBoil.AuthGuard.Application --startup-project MyApp.Api
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `DotBoil.Caching`, `DotBoil.Cors`, `DotBoil.EFCore`
- `DotBoil.Localization`, `DotBoil.Logging`, `DotBoil.Mapper`
- `DotBoil.MassTransit`, `DotBoil.Mediator`, `DotBoil.Parameter`, `DotBoil.Validator`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `NETCore.Encrypt`