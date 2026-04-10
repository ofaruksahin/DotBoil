# DotBoil.Cors

ASP.NET Core CORS politikasını `appsettings.json` üzerinden yapılandırır. İzin verilen origin'leri merkezi olarak yönetir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Cors\DotBoil.Cors.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Cors": {
      "PolicyName": "DefaultPolicy",
      "Origins": [
        "https://app.example.com",
        "https://admin.example.com"
      ]
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `PolicyName` | `string` | CORS politikasının adı |
| `Origins` | `string[]` | İzin verilen origin listesi |

### Geliştirme Ortamı

```json
{
  "DotBoil": {
    "Cors": {
      "PolicyName": "DevPolicy",
      "Origins": [
        "http://localhost:3000",
        "http://localhost:5173"
      ]
    }
  }
}
```

---

## Çalışma Mantığı

- `AddDotBoil()` → `Services.AddCors()` ile politikayı servis konteynerine kaydeder.
- `UseDotBoil()` → `app.UseCors(policyName)` middleware'ini pipeline'a ekler.
- Tüm origin'ler için `AllowAnyHeader()` ve `AllowAnyMethod()` otomatik olarak eklenir.

---

## Notlar

- Wildcard `*` origin kullanımı önerilmez; production'da her zaman explicit domain listesi belirtin.
- Birden fazla ortam için `appsettings.Development.json` ve `appsettings.Production.json` dosyalarında farklı origin listeleri tanımlayabilirsiniz.

---

## Bağımlılıklar

- `DotBoil` (core)