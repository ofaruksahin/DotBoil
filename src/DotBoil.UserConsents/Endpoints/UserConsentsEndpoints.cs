using System.Net;
using System.Security.Claims;
using DotBoil;
using DotBoil.Entities;
using DotBoil.UserConsents.Models;
using DotBoil.UserConsents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.UserConsents.Endpoints
{
    internal static class UserConsentsEndpoints
    {
        private const string RoutePrefix = "/api/consents";

        public static IEndpointRouteBuilder MapUserConsentsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var adminGroup = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("UserConsents")
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();

            adminGroup.MapGet(string.Empty, GetConsents);
            adminGroup.MapGet("/{id:int}", GetConsent);
            adminGroup.MapPost(string.Empty, CreateConsent);
            adminGroup.MapPut("/{id:int}", UpdateConsent);
            adminGroup.MapDelete("/{id:int}", DeleteConsent);

            var publicGroup = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("UserConsents");

            publicGroup.MapGet("/check", CheckUserConsent);
            publicGroup.MapPost("/accept", AcceptConsent);

            return endpoints;
        }

        // --- Admin endpoints ---

        private static async Task<IResult> GetConsents(
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var consents = await service.GetAllConsentsInternal(cancellationToken);
            return JsonResponse(BaseResponse.Response(consents, HttpStatusCode.OK));
        }

        private static async Task<IResult> GetConsent(
            int id,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var consents = await service.GetAllConsentsInternal(cancellationToken);
            var consent = consents.FirstOrDefault(c => c.Id == id);

            if (consent is null)
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Consent not found."));

            return JsonResponse(BaseResponse.Response(consent, HttpStatusCode.OK));
        }

        private static async Task<IResult> CreateConsent(
            CreateConsentRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidateCreate(request);
            if (validationResult is not null)
                return validationResult;

            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var consent = await service.CreateConsentInternal(request, cancellationToken);

            return JsonResponse(BaseResponse.Response(
                new ConsentMutationResponse { Id = consent.Id, Version = consent.Version },
                HttpStatusCode.Created,
                "Consent created successfully."));
        }

        private static async Task<IResult> UpdateConsent(
            int id,
            UpdateConsentRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var validationResult = ValidateUpdate(request);
            if (validationResult is not null)
                return validationResult;

            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var consent = await service.UpdateConsentInternal(id, request, cancellationToken);

            if (consent is null)
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Consent not found."));

            return JsonResponse(BaseResponse.Response(
                new ConsentMutationResponse { Id = consent.Id, Version = consent.Version },
                HttpStatusCode.OK,
                "Consent updated successfully."));
        }

        private static async Task<IResult> DeleteConsent(
            int id,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var deleted = await service.DeleteConsentInternal(id, cancellationToken);

            if (!deleted)
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Consent not found."));

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Consent deleted successfully."));
        }

        // --- Public endpoints ---

        private static async Task<IResult> CheckUserConsent(
            ConsentType type,
            string language,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();
            var result = await service.CheckUserConsentInternal(userId, type, language, cancellationToken);

            return JsonResponse(BaseResponse.Response((object)result, HttpStatusCode.OK));
        }

        private static async Task<IResult> AcceptConsent(
            AcceptConsentRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

            if (string.IsNullOrWhiteSpace(request.Language))
                return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Language is required."));

            var service = httpContext.RequestServices.GetRequiredService<UserConsentsService>();

            try
            {
                await service.AcceptConsent(userId, request.Type, request.Language, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, ex.Message));
            }

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Consent accepted successfully."));
        }

        // --- Validation ---

        private static IResult ValidateCreate(CreateConsentRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Language))
                errors[nameof(request.Language)] = ["Language is required."];
            else if (request.Language.Trim().Length > 20)
                errors[nameof(request.Language)] = ["Language can be at most 20 characters."];

            if (string.IsNullOrWhiteSpace(request.Content))
                errors[nameof(request.Content)] = ["Content is required."];

            return errors.Count > 0
                ? JsonResponse(BaseResponse.Response(errors, HttpStatusCode.BadRequest,
                    errors.SelectMany(e => e.Value).ToArray()))
                : null;
        }

        private static IResult ValidateUpdate(UpdateConsentRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Language))
                errors[nameof(request.Language)] = ["Language is required."];
            else if (request.Language.Trim().Length > 20)
                errors[nameof(request.Language)] = ["Language can be at most 20 characters."];

            if (string.IsNullOrWhiteSpace(request.Content))
                errors[nameof(request.Content)] = ["Content is required."];

            return errors.Count > 0
                ? JsonResponse(BaseResponse.Response(errors, HttpStatusCode.BadRequest,
                    errors.SelectMany(e => e.Value).ToArray()))
                : null;
        }

        private static IResult JsonResponse(BaseResponse response)
            => Results.Json(response, statusCode: (int)response.StatusCode);

        private sealed class ConsentMutationResponse
        {
            public int Id { get; init; }
            public int Version { get; init; }
        }

        private sealed class AcceptConsentRequest
        {
            public ConsentType Type { get; set; }
            public string Language { get; set; }
        }
    }
}