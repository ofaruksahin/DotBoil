# DotBoil.Validator.Mediator

MediatR pipeline'ında FluentValidation entegrasyonu. Her request handler çalışmadan önce ilgili validator otomatik bulunur ve çalıştırılır.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Validator.Mediator\DotBoil.Validator.Mediator.csproj" />
```

---

## Konfigürasyon

`DotBoil.Mediator` pipeline listesine `ValidationBehaviour` ekleyin:

```json
{
  "DotBoil": {
    "Mediator": {
      "Pipelines": [
        "DotBoil.Logging.LoggingBehaviour",
        "DotBoil.Validator.ValidationBehaviour"
      ]
    }
  }
}
```

`LoggingBehaviour` dışta (önce), `ValidationBehaviour` içte (sonra) çalışacak şekilde sıralayın. Bu sayede validasyon hataları da loglanmış olur.

---

## Çalışma Şekli

1. MediatR isteği alır
2. `ValidationBehaviour<TRequest, TResponse>` pipeline'da devreye girer
3. `IValidator<TRequest>` DI konteynerinden çözümlenir
4. `ValidateAsync()` çağrılır
5. Hatalar varsa `ValidationException` fırlatılır — handler çalışmaz
6. Hatalar yoksa bir sonraki pipeline halkasına geçilir

---

## Hata Yönetimi

`ValidationException` fırlatıldığında HTTP 400 yanıtı dönmesi için global exception handler ekleyin:

```csharp
app.UseExceptionHandler(exHandler =>
{
    exHandler.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            await context.Response.WriteAsJsonAsync(new
            {
                Type    = "ValidationError",
                Message = "Giriş doğrulama başarısız.",
                Errors  = errors
            });
        }
    });
});
```

Örnek yanıt:

```json
{
  "type": "ValidationError",
  "message": "Giriş doğrulama başarısız.",
  "errors": {
    "Name": ["Ürün adı boş olamaz.", "Ürün adı 200 karakterden uzun olamaz."],
    "Price": ["Fiyat 0'dan büyük olmalıdır."]
  }
}
```

---

## Bağımlılıklar

- `DotBoil.Mediator`
- `DotBoil.Validator`