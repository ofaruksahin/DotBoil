# DotBoil.Versioning

ASP.NET Core Minimal API versiyonlamasını yapılandırır. URL segment ve header tabanlı versiyonlamayı destekler; `DotBoil.Swag` ile birlikte her versiyon ayrı Swagger UI sayfasına sahip olur.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Versioning\DotBoil.Versioning.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Versioning": {
      "UrlSegmentApiVersioningEnable": true,
      "HeaderApiVersioningEnable": false,
      "HeaderApiVersioningHeaderName": "X-Api-Version",
      "DefaultMajorVersion": 1,
      "DefaultMinorVersion": 0
    }
  }
}
```

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `UrlSegmentApiVersioningEnable` | `bool` | `true` | URL'de `/v1/` segmenti ile versiyonlama |
| `HeaderApiVersioningEnable` | `bool` | `false` | HTTP header ile versiyonlama |
| `HeaderApiVersioningHeaderName` | `string` | `X-Api-Version` | Header tabanlı versiyonlama için header adı |
| `DefaultMajorVersion` | `int` | `1` | Versiyon belirtilmediğinde kullanılan major versiyon |
| `DefaultMinorVersion` | `int` | `0` | Versiyon belirtilmediğinde kullanılan minor versiyon |

---

## Versiyonlu Endpoint Tanımlama

```csharp
var v1 = app.NewApiVersionSet()
    .HasApiVersion(1, 0)
    .HasApiVersion(2, 0)
    .ReportApiVersions()
    .Build();

// v1 endpoint'i
app.MapGet("/api/v{version:apiVersion}/products", GetProductsV1)
   .WithApiVersionSet(v1)
   .MapToApiVersion(1, 0);

// v2 endpoint'i (değişiklikler içerir)
app.MapGet("/api/v{version:apiVersion}/products", GetProductsV2)
   .WithApiVersionSet(v1)
   .MapToApiVersion(2, 0);
```

### Grup Bazlı Versiyonlama

```csharp
var apiV1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(v1)
    .MapToApiVersion(1, 0);

var apiV2 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(v1)
    .MapToApiVersion(2, 0);

apiV1.MapGet("/products", GetProductsV1);
apiV2.MapGet("/products", GetProductsV2);
apiV2.MapPost("/products", CreateProductV2);
```

---

## Header Tabanlı Versiyonlama

```json
{
  "DotBoil": {
    "Versioning": {
      "UrlSegmentApiVersioningEnable": false,
      "HeaderApiVersioningEnable": true,
      "HeaderApiVersioningHeaderName": "X-Api-Version"
    }
  }
}
```

İstek başlığı:
```
GET /api/products
X-Api-Version: 2.0
```

---

## DotBoil.Swag ile Entegrasyon

```json
{
  "DotBoil": {
    "Versioning": {
      "UrlSegmentApiVersioningEnable": true,
      "DefaultMajorVersion": 1
    },
    "Swagger": {
      "Versions": [
        { "VersionName": "v1", "VersionDescription": "Stabil" },
        { "VersionName": "v2", "VersionDescription": "Beta" }
      ]
    }
  }
}
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `Asp.Versioning.Http 8.1.1`
- `Asp.Versioning.Mvc.ApiExplorer 8.1.1`