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
