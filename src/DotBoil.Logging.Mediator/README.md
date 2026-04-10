# DotBoil.Logging.Mediator

MediatR pipeline'ında request/response loglama. Her isteğin giriş, çıkış ve süre bilgilerini Serilog üzerinden loglar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Logging.Mediator\DotBoil.Logging.Mediator.csproj" />
```

---

## Konfigürasyon

`DotBoil.Mediator` pipeline listesine ekleyin:

```json
{
  "DotBoil": {
    "Mediator": {
      "Pipelines": [
        "DotBoil.Logging.LoggingBehaviour"
      ]
    }
  }
}
```

Birden fazla behavior varsa sıra önemlidir — loglama tipik olarak en dışta olmalıdır:

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

---

## Log Çıktısı

Her MediatR request için şu bilgiler loglanır:

```
[INF] Handling GetProductQuery { ProductId: 42 }
[INF] Handled GetProductQuery in 12ms → { Id: 42, Name: "Laptop", Price: 15000 }
```

Hata durumunda:

```
[ERR] GetProductQuery failed after 8ms → ProductNotFoundException: Product 42 not found
```

---

## Bağımlılıklar

- `DotBoil.Mediator`
- `DotBoil.Logging` (runtime'da inject edilir)