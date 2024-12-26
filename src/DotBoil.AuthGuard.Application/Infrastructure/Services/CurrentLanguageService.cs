using DotBoil.Localization;
using Microsoft.AspNetCore.Http;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class CurrentLanguageService : ICurrentLanguage
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentLanguageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string Language => _httpContextAccessor.HttpContext.Request.Query.ContainsKey("language") 
        ? _httpContextAccessor.HttpContext.Request.Query["language"]
        : "EN";
}