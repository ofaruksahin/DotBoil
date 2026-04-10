# DotBoil.Mediator

MediatR entegrasyonu. Request handler'ları ve pipeline behavior'ları otomatik keşfeder. Performans izleme behavior'ı dahil gelir.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Mediator\DotBoil.Mediator.csproj" />
```

---

## Konfigürasyon

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

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Pipelines` | `string[]` | Sıraya eklenecek pipeline behavior'larının tam tip adları |

Pipeline davranışları sırasıyla çalışır; ilk girilecek behavior dıştaki halka olur.

---

## Request Tanımlama

### Yanıtsız Command

```csharp
public class DeleteProductCommand : IRequest
{
    public int ProductId { get; set; }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IRepository<Product, AppDbContext> _repository;

    public DeleteProductCommandHandler(IRepository<Product, AppDbContext> repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId);
        _repository.Remove(product);
        await _repository.SaveChangesAsync();
    }
}
```

### Yanıtlı Query

```csharp
public class GetProductQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IRepository<Product, AppDbContext> _repository;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(
        IRepository<Product, AppDbContext> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId);
        return _mapper.Map<ProductDto>(product);
    }
}
```

---

## ISender Kullanımı

```csharp
public class ProductEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/products/{id}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductQuery { ProductId = id });
            return Results.Ok(result);
        });

        app.MapDelete("/products/{id}", async (int id, ISender sender) =>
        {
            await sender.Send(new DeleteProductCommand { ProductId = id });
            return Results.NoContent();
        });
    }
}
```

---

## Özel Pipeline Behavior

```csharp
public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICache _cache;

    public CachingBehaviour(ICache cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableRequest cacheableRequest)
            return await next();

        var cacheKey = cacheableRequest.CacheKey;
        return await _cache.GetOrSetAsync(
            cacheKey,
            () => next(),
            cacheableRequest.CacheDuration
        );
    }
}
```

```json
{
  "DotBoil": {
    "Mediator": {
      "Pipelines": [
        "MyApp.Application.Behaviours.CachingBehaviour`2"
      ]
    }
  }
}
```

---

## Dahili Pipeline Behavior'lar

### PerformanceBehaviour

Yavaş request'leri (varsayılan: 500ms üzeri) loglar:

```
[PERFORMANCE] GetProductQuery 650ms — Yavaş request tespit edildi!
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `MediatR 14.0.0`