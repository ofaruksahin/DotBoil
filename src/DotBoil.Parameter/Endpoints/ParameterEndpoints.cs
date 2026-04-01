using System.Net;
using DotBoil.Entities;
using DotBoil.Parameter.Dtos;
using DotBoil.Parameter.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Parameter.Endpoints
{
    internal static class ParameterEndpoints
    {
        private const string RoutePrefix = "/api/parameters";

        public static IEndpointRouteBuilder MapParameterEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints
                .MapGroup(RoutePrefix)
                .WithTags("Parameter");

            group.MapGet(string.Empty, GetParameters);
            group.MapGet("/{id:int}", GetParameter)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();
            group.MapPost(string.Empty, CreateParameter)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();
            group.MapPut("/{id:int}", UpdateParameter)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();
            group.MapDelete("/{id:int}", DeleteParameter)
                .WithMetadata(new CheckRoleAttribute("Admin"))
                .AddEndpointFilter<CheckRoleEndpointFilter>();

            return endpoints;
        }

        private static async Task<IResult> GetParameters(
            HttpContext httpContext,
            ParameterDbContext dbContext,
            int? tenantId,
            string? section,
            string? key,
            bool? isPublic,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Parameters.AsQueryable();

            if (tenantId.HasValue)
            {
                query = query.Where(parameter => parameter.TenantId == tenantId.Value);
            }

            if (section is not null)
            {
                var normalizedSection = NormalizeSection(section);
                query = query.Where(parameter => (parameter.Section ?? string.Empty) == normalizedSection);
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                var normalizedKey = key.Trim();
                query = query.Where(parameter => parameter.Key == normalizedKey);
            }

            if (isPublic.HasValue)
            {
                if (!isPublic.Value)
                {
                    isPublic = !httpContext.User.CheckRole("Admin");
                }
                
                query = query.Where(parameter => parameter.IsPublic == isPublic.Value);
            }
            else
            {
                isPublic = true;
            }

            var parameters = await query
                .OrderBy(parameter => parameter.TenantId)
                .ThenBy(parameter => parameter.Section)
                .ThenBy(parameter => parameter.Key)
                .ThenBy(parameter => parameter.IsPublic)
                .Select(parameter => new ParameterResponse
                {
                    Id = parameter.Id,
                    TenantId = parameter.TenantId,
                    Section = parameter.Section ?? string.Empty,
                    Key = parameter.Key,
                    Value = parameter.Value,
                    IsPublic = parameter.IsPublic
                })
                .ToListAsync(cancellationToken);

            return JsonResponse(BaseResponse.Response(parameters, HttpStatusCode.OK));
        }

        private static async Task<IResult> GetParameter(
            int id,
            ParameterDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var parameter = await dbContext.Parameters
                .Where(item => item.Id == id)
                .Select(item => new ParameterResponse
                {
                    Id = item.Id,
                    TenantId = item.TenantId,
                    Section = item.Section ?? string.Empty,
                    Key = item.Key,
                    Value = item.Value,
                    IsPublic = item.IsPublic
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (parameter is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Parameter not found."));
            }

            return JsonResponse(BaseResponse.Response(parameter, HttpStatusCode.OK));
        }

        private static async Task<IResult> CreateParameter(
            SaveParameterRequest request,
            ParameterDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var validationResult = Validate(request);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var normalizedRequest = Normalize(request);
            var exists = await dbContext.Parameters.AnyAsync(
                item => item.TenantId == normalizedRequest.TenantId &&
                        (item.Section ?? string.Empty) == normalizedRequest.Section &&
                        item.Key == normalizedRequest.Key &&
                        item.IsPublic == normalizedRequest.IsPublic,
                cancellationToken);

            if (exists)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Parameter already exists."));
            }

            var parameter = new Models.Parameter
            {
                TenantId = normalizedRequest.TenantId,
                Section = normalizedRequest.Section,
                Key = normalizedRequest.Key,
                Value = normalizedRequest.Value,
                IsPublic = normalizedRequest.IsPublic
            };

            await dbContext.Parameters.AddAsync(parameter, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(
                BaseResponse.Response(
                    ToResponse(parameter),
                    HttpStatusCode.Created,
                    "Parameter created successfully."));
        }

        private static async Task<IResult> UpdateParameter(
            int id,
            SaveParameterRequest request,
            ParameterDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var validationResult = Validate(request);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var parameter = await dbContext.Parameters
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (parameter is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Parameter not found."));
            }

            var normalizedRequest = Normalize(request);
            var exists = await dbContext.Parameters.AnyAsync(
                item => item.Id != id &&
                        item.TenantId == normalizedRequest.TenantId &&
                        (item.Section ?? string.Empty) == normalizedRequest.Section &&
                        item.Key == normalizedRequest.Key &&
                        item.IsPublic == normalizedRequest.IsPublic,
                cancellationToken);

            if (exists)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Parameter already exists."));
            }

            parameter.TenantId = normalizedRequest.TenantId;
            parameter.Section = normalizedRequest.Section;
            parameter.Key = normalizedRequest.Key;
            parameter.Value = normalizedRequest.Value;
            parameter.IsPublic = normalizedRequest.IsPublic;

            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(
                BaseResponse.Response(
                    ToResponse(parameter),
                    HttpStatusCode.OK,
                    "Parameter updated successfully."));
        }

        private static async Task<IResult> DeleteParameter(
            int id,
            ParameterDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var parameter = await dbContext.Parameters
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (parameter is null)
            {
                return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Parameter not found."));
            }

            dbContext.Parameters.Remove(parameter);
            await dbContext.SaveChangesAsync(cancellationToken);

            return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Parameter deleted successfully."));
        }

        private static IResult? Validate(SaveParameterRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (request.TenantId < 0)
            {
                errors[nameof(request.TenantId)] = ["TenantId cannot be negative."];
            }

            if ((request.Section ?? string.Empty).Length > 128)
            {
                errors[nameof(request.Section)] = ["Section can be at most 128 characters."];
            }

            if (string.IsNullOrWhiteSpace(request.Key))
            {
                errors[nameof(request.Key)] = ["Key is required."];
            }
            else if (request.Key.Trim().Length > 2048)
            {
                errors[nameof(request.Key)] = ["Key can be at most 2048 characters."];
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

        private static SaveParameterRequest Normalize(SaveParameterRequest request)
        {
            return new SaveParameterRequest
            {
                TenantId = request.TenantId,
                Section = NormalizeSection(request.Section),
                Key = request.Key.Trim(),
                Value = request.Value.Trim(),
                IsPublic = request.IsPublic
            };
        }

        private static string NormalizeSection(string? section)
        {
            return string.IsNullOrWhiteSpace(section)
                ? string.Empty
                : section.Trim();
        }

        private static ParameterResponse ToResponse(Models.Parameter parameter)
        {
            return new ParameterResponse
            {
                Id = parameter.Id,
                TenantId = parameter.TenantId,
                Section = parameter.Section ?? string.Empty,
                Key = parameter.Key,
                Value = parameter.Value,
                IsPublic = parameter.IsPublic
            };
        }

        private static IResult JsonResponse(BaseResponse response)
        {
            return Results.Json(response, statusCode: (int)response.StatusCode);
        }
    }
}
