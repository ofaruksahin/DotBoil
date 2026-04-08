using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class ExternalLoginEndpoints
{
    public static IEndpointRouteBuilder MapExternalLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/connect/{provider}", InitiateExternalLogin)
            .WithTags("ExternalLogin");

        endpoints.MapGet("/connect/external/{provider}", HandleExternalLoginCallback)
            .WithTags("ExternalLogin");

        return endpoints;
    }

    private static IResult InitiateExternalLogin(
        string provider,
        IConfiguration configuration)
    {
        var managers = GetExternalSignInManagers(configuration);
        var manager = managers.FirstOrDefault(m => m.ServiceName == provider);

        if (manager is null)
            return Results.Problem(
                detail: $"'{provider}' provider is not supported.",
                statusCode: StatusCodes.Status400BadRequest);

        return Results.Redirect(manager.AuthorizationEndpoint);
    }

    private static async Task<IResult> HandleExternalLoginCallback(
        string provider,
        HttpContext httpContext,
        IConfiguration configuration,
        IExternalSignInManager externalSignInManager)
    {
        var code = httpContext.Request.Query["code"].ToString();

        if (string.IsNullOrEmpty(code))
            return Results.Problem(
                detail: "Authorization code is missing.",
                statusCode: StatusCodes.Status400BadRequest);

        var managers = GetExternalSignInManagers(configuration);
        var manager = managers.FirstOrDefault(m => m.ServiceName == provider);

        if (manager is null)
            return Results.Problem(
                detail: $"'{provider}' provider is not supported.",
                statusCode: StatusCodes.Status400BadRequest);

        var tokenEndpoint = $"{manager.TokenEndpoint}&code={code}";

        var loginRequest = new ExternalLoginRequest(
            provider,
            tokenEndpoint,
            manager.UserInfoEndpoint,
            manager.EmailIdentifier,
            manager.UsernameIdentifier,
            manager.NameIdentifier,
            manager.SurnameIdentifier);

        var result = await externalSignInManager.ExternalLogin(loginRequest);

        if (!result.IsSuccess)
            return Results.Problem(
                detail: "External login authorization failed.",
                statusCode: StatusCodes.Status401Unauthorized);

        var redirectUri = BuildRedirectUri(manager.RedirectUrl, result);

        return Results.Redirect(redirectUri);
    }

    private static string BuildRedirectUri(string baseUri, AuthorizeResult result)
        => $"{baseUri}?access_token={result.AccessToken}" +
           $"&refresh_token={result.RefreshToken}" +
           $"&expire_access_token={result.ExpireAccessToken:dd/MM/yyyy-HH:mm}" +
           $"&expire_refresh_token={result.ExpireRefreshToken:dd/MM/yyyy-HH:mm}";

    private static List<ExternalSignInManagerConfig> GetExternalSignInManagers(IConfiguration configuration)
        => configuration
               .GetSection("DotBoil:ExternalSignInManagers")
               .Get<List<ExternalSignInManagerConfig>>()
           ?? new List<ExternalSignInManagerConfig>();

    private sealed class ExternalSignInManagerConfig
    {
        public string ServiceName { get; set; } = string.Empty;
        public string AuthorizationEndpoint { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string UserInfoEndpoint { get; set; } = string.Empty;
        public string EmailIdentifier { get; set; } = string.Empty;
        public string UsernameIdentifier { get; set; } = string.Empty;
        public string NameIdentifier { get; set; } = string.Empty;
        public string SurnameIdentifier { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}