using System.Net;
using DotBoil;
using DotBoil.Entities;
using DotBoil.Enums;
using DotBoil.Localization.Dtos;
using DotBoil.Localization.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Localization.Endpoints
{
    internal static class LocalizationEndpoints
    {
        private const string RoutePrefix = "/api/localizations";

        public static IEndpointRouteBuilder MapLocalizationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("Localization");

            group.MapGet(string.Empty, GetLocalizations);
            group.MapGet("/{id:int}", GetLocalization);
            group.MapPost(string.Empty, CreateLocalization)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();
            group.MapPut("/{id:int}", UpdateLocalization)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();
            group.MapDelete("/{id:int}", DeleteLocalization)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();

            return endpoints;
        }

        private static async Task<IResult> GetLocalizations(
            LocalizationDbContext dbContext,
            string? language = null,
            string? group = null,
            string? key = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? sortColumn = null,
            EnumSortDirection sortDirection = EnumSortDirection.Ascending,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Localizations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(language))
                query = query.Where(localization => localization.Language == language.Trim());

            if (group is not null)
            {
                var normalizedGroup = NormalizeGroup(group);
                query = query.Where(localization => (localization.Group ?? string.Empty) == normalizedGroup);
            }

            if (!string.IsNullOrWhiteSpace(key))
                query = query.Where(localization => localization.Key == key.Trim());

            var ordered = sortColumn?.ToLowerInvariant() switch
            {
                "language" => sortDirection == EnumSortDirection.Descending
                    ? query.OrderByDescending(l => l.Language)
                    : query.OrderBy(l => l.Language),
                "group"    => sortDirection == EnumSortDirection.Descending
                    ? query.OrderByDescending(l => l.Group)
                    : query.OrderBy(l => l.Group),
                "key"      => sortDirection == EnumSortDirection.Descending
                    ? query.OrderByDescending(l => l.Key)
                    : query.OrderBy(l => l.Key),
                _          => query
                    .OrderBy(l => l.Language)
                    .ThenBy(l => l.Group)
                    .ThenBy(l => l.Key)
            };

            var totalRecords = await ordered.CountAsync(cancellationToken);
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(localization => new LocalizationResponse
                {
                    Id       = localization.Id,
                    Language = localization.Language,
                    Group    = localization.Group ?? string.Empty,
                    Key      = localization.Key,
                    Value    = localization.Value
                })
                .ToListAsync(cancellationToken);

            var result = new PaginatedModel<LocalizationResponse>(pageNumber, pageSize, totalPages, totalRecords, items);
            return JsonResponse(BaseResponse.Response(result, HttpStatusCode.OK));
        }

        private static async Task<IResult> GetLocalization(
            int id,
            LocalizationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var localization = await dbContext.Localizations
                .Where(item => item.Id == id)
                .Select(item => new LocalizationResponse
                {
                    Id = item.Id,
                    Language = item.Language,
                    Group = item.Group ?? string.Empty,
                    Key = item.Key,
                    Value = item.Value
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (localization is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Localization not found."));
            }

            return JsonResponse(BaseResponse.Response(localization, HttpStatusCode.OK));
        }

        private static async Task<IResult> CreateLocalization(
            SaveLocalizationRequest request,
            LocalizationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var validationResult = Validate(request);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var normalizedRequest = Normalize(request);
            var exists = await dbContext.Localizations.AnyAsync(
                item => item.Language == normalizedRequest.Language &&
                        (item.Group ?? string.Empty) == normalizedRequest.Group &&
                        item.Key == normalizedRequest.Key,
                cancellationToken);

            if (exists)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Localization already exists."));
            }

            var localization = new Models.Localization
            {
                Language = normalizedRequest.Language,
                Group = normalizedRequest.Group,
                Key = normalizedRequest.Key,
                Value = normalizedRequest.Value
            };

            await dbContext.Localizations.AddAsync(localization, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(
                BaseResponse.Response(
                    ToResponse(localization),
                    HttpStatusCode.Created,
                    "Localization created successfully."));
        }

        private static async Task<IResult> UpdateLocalization(
            int id,
            SaveLocalizationRequest request,
            LocalizationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var validationResult = Validate(request);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var localization = await dbContext.Localizations
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (localization is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Localization not found."));
            }

            var normalizedRequest = Normalize(request);
            var exists = await dbContext.Localizations.AnyAsync(
                item => item.Id != id &&
                        item.Language == normalizedRequest.Language &&
                        (item.Group ?? string.Empty) == normalizedRequest.Group &&
                        item.Key == normalizedRequest.Key,
                cancellationToken);

            if (exists)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Localization already exists."));
            }

            localization.Language = normalizedRequest.Language;
            localization.Group = normalizedRequest.Group;
            localization.Key = normalizedRequest.Key;
            localization.Value = normalizedRequest.Value;

            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(
                BaseResponse.Response(
                    ToResponse(localization),
                    HttpStatusCode.OK,
                    "Localization updated successfully."));
        }

        private static async Task<IResult> DeleteLocalization(
            int id,
            LocalizationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var localization = await dbContext.Localizations
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (localization is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Localization not found."));
            }

            dbContext.Localizations.Remove(localization);
            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Localization deleted successfully."));
        }

        private static IResult Validate(SaveLocalizationRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Language))
            {
                errors[nameof(request.Language)] = ["Language is required."];
            }
            else if (request.Language.Trim().Length > 20)
            {
                errors[nameof(request.Language)] = ["Language can be at most 20 characters."];
            }

            if (!string.IsNullOrWhiteSpace(request.Group) && request.Group.Trim().Length > 100)
            {
                errors[nameof(request.Group)] = ["Group can be at most 100 characters."];
            }

            if (string.IsNullOrWhiteSpace(request.Key))
            {
                errors[nameof(request.Key)] = ["Key is required."];
            }
            else if (request.Key.Trim().Length > 100)
            {
                errors[nameof(request.Key)] = ["Key can be at most 100 characters."];
            }

            if (string.IsNullOrWhiteSpace(request.Value))
            {
                errors[nameof(request.Value)] = ["Value is required."];
            }

            return errors.Count > 0
                ? JsonResponse(
                    BaseResponse.Response(
                        errors,
                        HttpStatusCode.BadRequest,
                        errors.SelectMany(item => item.Value).ToArray()))
                : null;
        }

        private static IResult JsonResponse(BaseResponse response)
        {
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }

        private static SaveLocalizationRequest Normalize(SaveLocalizationRequest request)
        {
            return new SaveLocalizationRequest
            {
                Language = request.Language.Trim(),
                Group = NormalizeGroup(request.Group),
                Key = request.Key.Trim(),
                Value = request.Value.Trim()
            };
        }

        private static string NormalizeGroup(string group)
        {
            return string.IsNullOrWhiteSpace(group)
                ? string.Empty
                : group.Trim();
        }

        private static LocalizationResponse ToResponse(Models.Localization localization)
        {
            return new LocalizationResponse
            {
                Id = localization.Id,
                Language = localization.Language,
                Group = localization.Group ?? string.Empty,
                Key = localization.Key,
                Value = localization.Value
            };
        }
    }
}
