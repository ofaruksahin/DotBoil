using System.Net;
using System.Security.Claims;
using DotBoil.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotBoil;

public class CheckAppModuleAuthorizationFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();

        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>();

        if (allowAnonymous is not null)
            return;

        var appModuleAttributes = endpoint?.Metadata.GetOrderedMetadata<CheckAppModuleAttribute>();

        if (appModuleAttributes is null || !appModuleAttributes.Any())
            return;

        var user = context.HttpContext.User;

        if (user?.Identity.IsAuthenticated != true)
        {
            var response = new BaseResponse(new { }, HttpStatusCode.Unauthorized);
            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)response.StatusCode   
            };
            
            return;
        }

        var appModules = user.Claims
            .Where(c => c.Type == DotBoilClaimTypes.AppModule)
            .Select(c => c.Value)
            .ToList();

        var hasAppModule = false;

        foreach (var appModuleAttribute in appModuleAttributes)
        {
            if (hasAppModule) break;

            hasAppModule = appModules.Contains(appModuleAttribute.AppModule);
        }

        if (!hasAppModule)
        {
            var response = new BaseResponse(new { }, HttpStatusCode.Forbidden);
            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)response.StatusCode   
            };
            
            return;
        }

        return;
    }
}