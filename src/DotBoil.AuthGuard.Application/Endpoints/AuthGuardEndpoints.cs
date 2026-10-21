using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using DotBoil.AuthGuard.Application.Domain.DomainEvents;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Dtos;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.EFCore;
using DotBoil.Entities;
using DotBoil.Parameter;
using NETCore.Encrypt;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class AuthGuardEndpoints
{
    private const string NotificationSettingsSection = "AuthGuard";
    private const string NotificationSettingsKey = "NotificationSettings";

    private static readonly JsonSerializerOptions NotificationSettingsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapAuthGuardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/connect/refresh_token", RefreshToken);

        var appGroup = endpoints
            .MapGroup("/app")
            .RequireAuthorization()
            .WithTags("AuthGuard");

        appGroup.MapPost("/menu", GetMenuList);
        appGroup.MapPost("/permission", CheckPermission);
        appGroup.MapPost("/userinfo", GetUserInfo);
        appGroup.MapPost("/roles", GetCurrentUserRoles);
        appGroup.MapPost("/modules", GetCurrentUserAppModules);

        appGroup.MapPut("/active-language", SetActiveLanguage);
        appGroup.MapPost("/change-password", ChangePassword);
        appGroup.MapPost("/account/delete", RequestAccountDeletion);
        appGroup.MapPost("/account/delete/confirm", ConfirmAccountDeletion);
        appGroup.MapPost("/account/activate", ActivateAccount).AllowAnonymous();
        appGroup.MapGet("/notification-settings", GetNotificationSettings);
        appGroup.MapPut("/notification-settings", UpdateNotificationSettings);

        return endpoints;
    }

    private static async Task<IResult> RefreshToken(
        [FromQuery] string refreshToken,
        IRefreshTokenService refreshTokenService)
    {
        var refreshTokenResult = await refreshTokenService.RefreshToken(refreshToken);

        return Results.Json(
            refreshTokenResult,
            statusCode: refreshTokenResult.IsSuccess
                ? StatusCodes.Status200OK
                : StatusCodes.Status401Unauthorized);
    }

    [Authorize]
    private static async Task<IResult> GetMenuList(IMenuService menuService)
    {
        var menuList = await menuService.GetMenuItems();
        return Results.Ok(menuList);
    }

    [Authorize]
    private static async Task<IResult> CheckPermission(
        [FromQuery] string controller,
        [FromQuery] string action,
        IPermissionService permissionService)
    {
        var hasPermission = await permissionService.CheckPermission(controller, action);

        return hasPermission
            ? Results.Ok()
            : Results.Forbid();
    }

    [Authorize]
    private static async Task<IResult> GetUserInfo(IServiceProvider serviceProvider)
    {
        var userService = serviceProvider.GetRequiredKeyedService<IUserService>("EmailPasswordBasedLogin");
        var getUserInfo = await userService.GetUserInfo();

        return getUserInfo.Claims.Any()
            ? Results.Ok(getUserInfo.Claims)
            : Results.Unauthorized();
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUserRoles(IPermissionService permissionService)
    {
        var roles = await permissionService.GetCurrentUserRoles();

        return Results.Ok(roles.Select(role => new CurrentUserRoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            IsDefault = role.IsDefault
        }));
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUserAppModules(IPermissionService permissionService)
    {
        var appModules = await permissionService.GetCurrentUserAppModules();

        return Results.Ok(appModules.Select(appModule => new CurrentUserAppModuleResponse
        {
            Id = appModule.Id,
            Name = appModule.Name,
            Description = appModule.Description
        }));
    }

    private static async Task<IResult> SetActiveLanguage(
        SetActiveLanguageRequest request,
        HttpContext httpContext,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        var language = request.Language?.Trim();

        if (string.IsNullOrWhiteSpace(language))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Language is required."));

        if (language.Length > 20)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Language can be at most 20 characters."));

        var affectedRows = await userRepository
            .Get()
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.ActiveLanguage, language), cancellationToken);

        if (affectedRows == 0)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Active language updated successfully."));
    }

    private static async Task<IResult> ChangePassword(
        ChangePasswordRequest request,
        HttpContext httpContext,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Current password is required."));

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "New password is required."));

        var user = await userRepository
            .GetAll()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        // External provider accounts have no local password to change.
        if (!string.IsNullOrEmpty(user.Provider))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Password cannot be changed for external accounts."));

        if (user.Password != EncryptProvider.Md5(request.CurrentPassword))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Current password is incorrect."));

        user.Password = EncryptProvider.Md5(request.NewPassword);
        user.ModifyUser = user.Id.ToString();
        user.UpdateTime = DateTime.Now;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Password changed successfully."));
    }

    private static async Task<IResult> RequestAccountDeletion(
        HttpContext httpContext,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        var user = await userRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsPassive, cancellationToken);

        if (user is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        await otpCodeRepository.CreateOtpCode(
            user.Id,
            OtpCodePurpose.AccountDeletion,
            code => new AccountDeletionOtpCodeCreatedDomainEvent(user.Email, code));

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Account deletion code has been sent."));
    }

    private static async Task<IResult> ConfirmAccountDeletion(
        ConfirmAccountDeletionRequest request,
        HttpContext httpContext,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        IConnectionMultiplexer redis,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        var user = await userRepository
            .GetAll()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsPassive, cancellationToken);

        if (user is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        var verificationResult = await otpCodeRepository.VerifyOtpCode(user.Id, OtpCodePurpose.AccountDeletion, request.OtpCode);
        if (verificationResult != OtpCodeVerificationResult.Valid)
            return OtpCodeErrorResponse(verificationResult);

        user.IsPassive = true;
        user.PassiveDate = DateTime.Now;
        user.ModifyUser = user.Id.ToString();
        user.UpdateTime = DateTime.Now;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        // Drop every session of the user: access tokens fail authentication and refresh tokens cannot be used.
        await redis.RevokeUserTokens(user.Id);

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Account has been deactivated."));
    }

    private static async Task<IResult> ActivateAccount(
        ActivateAccountRequest request,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Email is required."));

        if (string.IsNullOrWhiteSpace(request.OtpCode))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Otp code is required."));

        var email = request.Email.Trim();

        var user = await userRepository
            .GetAll()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.IsPassive, cancellationToken);

        // Same response for unknown and active accounts so the endpoint does not reveal which emails exist.
        if (user is null)
            return OtpCodeErrorResponse(OtpCodeVerificationResult.Invalid);

        var verificationResult = await otpCodeRepository.VerifyOtpCode(user.Id, OtpCodePurpose.AccountActivation, request.OtpCode.Trim());
        if (verificationResult != OtpCodeVerificationResult.Valid)
            return OtpCodeErrorResponse(verificationResult);

        user.IsPassive = false;
        user.PassiveDate = null;
        user.ModifyUser = user.Id.ToString();
        user.UpdateTime = DateTime.Now;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Account has been activated."));
    }

    private static async Task<IResult> GetNotificationSettings(
        HttpContext httpContext,
        IParameterManager parameterManager,
        IRepository<UserNotificationPreference, DotBoilAuthGuardDbContext> preferenceRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        var channels = await GetNotificationChannels(parameterManager);
        if (channels is null)
            return NotificationSettingsNotConfiguredResponse();

        var preferences = await preferenceRepository
            .Get()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Channel, p => p.IsEnabled, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var settings = channels.Select(channel => new NotificationSettingResponse
        {
            Name = channel.Name,
            Icon = channel.Icon,
            IsEnabled = preferences.TryGetValue(channel.Name, out var isEnabled) && isEnabled
        });

        return JsonResponse(BaseResponse.Response(settings, HttpStatusCode.OK));
    }

    private static async Task<IResult> UpdateNotificationSettings(
        UpdateNotificationSettingsRequest request,
        HttpContext httpContext,
        IParameterManager parameterManager,
        IRepository<UserNotificationPreference, DotBoilAuthGuardDbContext> preferenceRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(httpContext, out var userId))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Unauthorized, "User is not authenticated."));

        var channels = await GetNotificationChannels(parameterManager);
        if (channels is null)
            return NotificationSettingsNotConfiguredResponse();

        var requestedSettings = (request.Settings ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().IsEnabled, StringComparer.OrdinalIgnoreCase);

        if (requestedSettings.Count == 0)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "At least one notification setting is required."));

        var channelNames = channels.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownChannels = requestedSettings.Keys.Where(name => !channelNames.Contains(name)).ToList();

        if (unknownChannels.Count > 0)
            return JsonResponse(BaseResponse.Response(
                HttpStatusCode.BadRequest,
                $"Unknown notification channel(s): {string.Join(", ", unknownChannels)}."));

        var existingPreferences = await preferenceRepository
            .GetAll()
            .AsTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var (name, isEnabled) in requestedSettings)
        {
            // Store the channel name exactly as it is defined in the parameter.
            var channelName = channels.First(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)).Name;
            var existing = existingPreferences.FirstOrDefault(p => string.Equals(p.Channel, channelName, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                await preferenceRepository.AddAsync(new UserNotificationPreference
                {
                    UserId = userId,
                    Channel = channelName,
                    IsEnabled = isEnabled,
                    CreateUser = userId.ToString(),
                    CreateTime = DateTime.Now
                });

                continue;
            }

            existing.IsEnabled = isEnabled;
            existing.ModifyUser = userId.ToString();
            existing.UpdateTime = DateTime.Now;
            preferenceRepository.Update(existing);
        }

        await preferenceRepository.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Notification settings updated successfully."));
    }

    // Returns null when the NotificationSettings parameter is missing or is not a valid JSON array.
    private static async Task<List<NotificationChannel>?> GetNotificationChannels(IParameterManager parameterManager)
    {
        try
        {
            var json = await parameterManager.GetParameterValue<string>(NotificationSettingsSection, NotificationSettingsKey);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<List<NotificationChannel>>(json, NotificationSettingsJsonOptions)?
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IResult NotificationSettingsNotConfiguredResponse()
        => JsonResponse(BaseResponse.Response(
            HttpStatusCode.InternalServerError,
            $"'{NotificationSettingsSection}:{NotificationSettingsKey}' parameter is missing or is not a valid JSON array."));

    private static IResult OtpCodeErrorResponse(OtpCodeVerificationResult verificationResult)
        => verificationResult == OtpCodeVerificationResult.Expired
            ? JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Otp code has expired."))
            : JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Otp code is invalid."));

    private static bool TryGetUserId(HttpContext httpContext, out int userId)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}
