# DotBoil.Validator

FluentValidation entegrasyonu. `AbstractValidator<T>` türeyen tüm sınıfları otomatik keşfeder ve DI konteynerine kaydeder.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Validator\DotBoil.Validator.csproj" />
```

---

## Konfigürasyon

Ekstra konfigürasyon gerekmez. Modül, çalışma zamanında `AbstractValidator<T>`'den türeyen tüm sınıfları otomatik bulur.

---

## Validator Tanımlama

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(200).WithMessage("Ürün adı 200 karakterden uzun olamaz.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçilmelidir.");
    }
}
```

---

## Direkt Kullanım

```csharp
public class ProductService
{
    private readonly IValidator<CreateProductRequest> _validator;

    public ProductService(IValidator<CreateProductRequest> validator)
    {
        _validator = validator;
    }

    public async Task CreateAsync(CreateProductRequest request)
    {
        var result = await _validator.ValidateAsync(request);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage });
            throw new ValidationException(result.Errors);
        }

        // İş mantığı...
    }
}
```

---

## MediatR Pipeline ile Kullanım

`DotBoil.Validator.Mediator` eklendiğinde, MediatR handler çalışmadan önce validasyon otomatik gerçekleşir. Ayrı `_validator.ValidateAsync()` çağrısına gerek kalmaz.

```xml
<ProjectReference Include="..\DotBoil.Validator.Mediator\DotBoil.Validator.Mediator.csproj" />
```

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

Request handler içinde validasyon kodu yazmaya gerek kalmaz:

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    // ValidationBehaviour pipeline'da zaten çalıştı
    // Buraya ulaşıldıysa validasyon geçti demektir
    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product { Name = request.Name, Price = request.Price };
        // ...
        return product.Id;
    }
}
```

Validasyon hatası durumunda pipeline `ValidationException` fırlatır.

---

## Koşullu Kurallar

```csharp
public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .When(x => x.Status == OrderStatus.Shipped)
            .WithMessage("Kargo durumundaki siparişlerin takip numarası olmalıdır.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .When(x => x.Status == OrderStatus.Cancelled)
            .WithMessage("İptal gerekçesi belirtilmelidir.");
    }
}
```

---

## Kayıtlı Servisler

| Servis | Yaşam Süresi |
|--------|--------------|
| `IValidator<T>` | Scoped |

---

## Bağımlılıklar

- `DotBoil` (core)
- `FluentValidation 12.1.1`