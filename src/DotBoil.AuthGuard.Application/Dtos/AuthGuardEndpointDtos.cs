namespace DotBoil.AuthGuard.Application.Dtos;

internal sealed class CurrentUserRoleResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}

internal sealed class CurrentUserAppModuleResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

internal sealed class SetActiveLanguageRequest
{
    public string? Language { get; init; }
}

internal sealed class ChangePasswordRequest
{
    public string? CurrentPassword { get; init; }
    public string? NewPassword { get; init; }
}

internal sealed class ConfirmAccountDeletionRequest
{
    public string? OtpCode { get; init; }
}

internal sealed class ActivateAccountRequest
{
    public string? Email { get; init; }
    public string? OtpCode { get; init; }
}

internal sealed class NotificationChannel
{
    public string Name { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
}

internal sealed class NotificationSettingResponse
{
    public string Name { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}

internal sealed class UpdateNotificationSettingsRequest
{
    public IEnumerable<NotificationSettingItem> Settings { get; init; } = [];
}

internal sealed class NotificationSettingItem
{
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
