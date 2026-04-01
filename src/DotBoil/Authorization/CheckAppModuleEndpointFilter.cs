using System.Net;
using DotBoil.Entities;
using Microsoft.AspNetCore.Http;

namespace DotBoil;

public class CheckAppModuleEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();

        var appModuleAttributes = endpoint?
            .Metadata
            .GetOrderedMetadata<CheckAppModuleAttribute>();

        if (appModuleAttributes == null || !appModuleAttributes.Any())
            return await next(context);

        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            var response = new BaseResponse(new { }, HttpStatusCode.Unauthorized);
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }

        var appModules = user.Claims
            .Where(c => c.Type == DotBoilClaimTypes.AppModule)
            .Select(c => c.Value)
            .ToList();

        var hasAppModule = false;

        foreach (var appModuleAttribute in appModuleAttributes)
        {
            if (hasAppModule)
                break;

            hasAppModule = true;
        }

        if (!hasAppModule)
        {
            var response = new BaseResponse(new { }, HttpStatusCode.Forbidden);
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }

        return await next(context);
    }
}