# DotBoil.Swag

Swagger / OpenAPI dokümantasyonunu otomatik yapılandırır. API versiyonlamayla (`DotBoil.Versioning`) tam entegrasyon, XML yorum desteği ve iletişim bilgisi tanımı içerir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Swag\DotBoil.Swag.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Swagger": {
      "XmlFile": "MyApp.Api.xml",
      "Versions": [
        {
          "VersionName": "v1",
          "VersionDescription": "MyApp API v1"
        }
      ],
      "Contact": {
        "Name": "Geliştirici Ekibi",
        "Email": "dev@example.com",
        "Url": "https://example.com"
      }
    }
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `XmlFile` | `string` | XML dokümantasyon dosyasının adı |
| `Versions` | `List` | Swagger UI'da görünecek API versiyonları |
| `Versions[].VersionName` | `string` | Versiyon adı (ör: `v1`, `v2`) |
| `Versions[].VersionDescription` | `string` | Versiyon açıklaması |
| `Contact.Name` | `string` | İletişim kişisi / ekip adı |
| `Contact.Email` | `string` | İletişim e-postası |
| `Contact.Url` | `string` | İletişim URL'i |

---

## XML Dokümantasyonu Aktifleştirme

`.csproj` dosyanıza ekleyin:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

## Endpoint Dokümantasyonu

```csharp
/// <summary>
/// Ürünü ID'ye göre getirir.
/// </summary>
/// <param name="id">Ürün ID'si</param>
/// <returns>Ürün detay bilgisi</returns>
/// <response code="200">Ürün başarıyla getirildi</response>
/// <response code="404">Ürün bulunamadı</response>
app.MapGet("/products/{id}", async (int id, ISender sender) =>
{
    var result = await sender.Send(new GetProductQuery { ProductId = id });
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetProduct")
.WithOpenApi()
.Produces<ProductDto>(200)
.Produces(404);
```

---

## Çoklu Versiyon

`DotBoil.Versioning` ile birlikte kullanıldığında her versiyon ayrı bir Swagger UI sayfasına sahip olur:

```json
{
  "DotBoil": {
    "Swagger": {
      "Versions": [
        { "VersionName": "v1", "VersionDescription": "Stabil API" },
        { "VersionName": "v2", "VersionDescription": "Beta — Yeni özellikler" }
      ]
    }
  }
}
```

---

## Endpoint'ler

| URL | Açıklama |
|-----|----------|
| `/swagger` | Swagger UI (tarayıcı) |
| `/swagger/{version}/swagger.json` | OpenAPI JSON tanımı |

---

## Bağımlılıklar

- `DotBoil` (core)
- `Swashbuckle.AspNetCore.SwaggerGen 10.1.0`
- `Swashbuckle.AspNetCore.SwaggerUI 10.1.0`