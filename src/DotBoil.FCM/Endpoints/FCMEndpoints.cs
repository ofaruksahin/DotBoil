using System.Net;
using System.Security.Claims;
using DotBoil.Entities;
using DotBoil.FCM.Models;
using DotBoil.FCM.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.FCM.Endpoints
{
    internal static class FCMEndpoints
    {
        private const string RoutePrefix = "/api/fcm";

        public static IEndpointRouteBuilder MapFCMEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var adminGroup = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("FCM")
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();

            adminGroup.MapPost("/send", SendNotification);

            var publicGroup = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("FCM");

            publicGroup.MapPost("/tokens", RegisterToken);
            publicGroup.MapPost("/tokens/unregister", UnregisterToken);

            return endpoints;
        }

        // --- Admin endpoints ---

        private static async Task<IResult> SendNotification(
            SendNotificationRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            if (request.UserIds is null || request.UserIds.Count == 0)
                return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "At least one user id is required."));

            if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Body)
                                                        && (request.Data is null || request.Data.Count == 0))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Title, Body or Data is required."));

            var service = httpContext.RequestServices.GetRequiredService<FCMService>();
            var result = await service.SendToUsers(
                request.UserIds,
                new FCMNotification
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    Data = request.Data
                },
                cancellationToken);

            return JsonResponse(BaseResponse.Response(result, HttpStatusCode.OK));
        }

        // --- Public endpoints ---

        private static async Task<IResult> RegisterToken(
            RegisterTokenRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

            var validationResult = ValidateRegister(request);
            if (validationResult is not null)
                return validationResult;

            var service = httpContext.RequestServices.GetRequiredService<FCMService>();
            await service.RegisterToken(userId, request.Token, request.Platform, request.DeviceId, cancellationToken);

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Token registered successfully."));
        }

        private static async Task<IResult> UnregisterToken(
            UnregisterTokenRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

            if (string.IsNullOrWhiteSpace(request.Token))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Token is required."));

            var service = httpContext.RequestServices.GetRequiredService<FCMService>();
            await service.UnregisterTokenInternal(userId, request.Token, cancellationToken);

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Token unregistered successfully."));
        }

        // --- Validation ---

        private static IResult ValidateRegister(RegisterTokenRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Token))
                errors[nameof(request.Token)] = ["Token is required."];
            else if (request.Token.Trim().Length > UserDeviceToken.TokenMaxLength)
                errors[nameof(request.Token)] = [$"Token can be at most {UserDeviceToken.TokenMaxLength} characters."];

            if (!Enum.IsDefined(request.Platform))
                errors[nameof(request.Platform)] = ["Platform is invalid."];

            if (request.DeviceId?.Trim().Length > UserDeviceToken.DeviceIdMaxLength)
                errors[nameof(request.DeviceId)] = [$"DeviceId can be at most {UserDeviceToken.DeviceIdMaxLength} characters."];

            return errors.Count > 0
                ? JsonResponse(BaseResponse.Response(errors, HttpStatusCode.BadRequest,
                    errors.SelectMany(e => e.Value).ToArray()))
                : null;
        }

        private static IResult JsonResponse(BaseResponse response)
            => Results.Json(response, statusCode: (int)response.StatusCode);

        private sealed class RegisterTokenRequest
        {
            public string Token { get; set; }
            public DevicePlatform Platform { get; set; }
            public string DeviceId { get; set; }
        }

        private sealed class UnregisterTokenRequest
        {
            public string Token { get; set; }
        }

        private sealed class SendNotificationRequest
        {
            public List<string> UserIds { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string ImageUrl { get; set; }
            public Dictionary<string, string> Data { get; set; }
        }
    }
}
