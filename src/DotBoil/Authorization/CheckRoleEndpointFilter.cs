using System.Net;
using System.Security.Claims;
using DotBoil.Entities;
using Microsoft.AspNetCore.Http;

namespace DotBoil;

public class CheckRoleEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();

        var roleAttributes = endpoint?
            .Metadata
            .GetOrderedMetadata<CheckRoleAttribute>();

        if (roleAttributes == null || !roleAttributes.Any())
            return await next(context);

        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            var response = new BaseResponse(new {}, HttpStatusCode.Unauthorized);
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }
        
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var hasRole = false;

        foreach (var roleAttribute in roleAttributes)
        {
            if (hasRole)
                break;

            hasRole = userRoles.Contains(roleAttribute.Role);
        }

        if (!hasRole)
        {
            var response = new BaseResponse(new {}, HttpStatusCode.Forbidden);
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }
        
        return await next(context);
    }
}