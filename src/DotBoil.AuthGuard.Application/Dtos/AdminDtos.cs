namespace DotBoil.AuthGuard.Application.Dtos;

// ApiEndpoint DTOs
internal sealed class ApiEndpointResponse
{
    public int Id { get; init; }
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public IEnumerable<int> AppModuleIds { get; init; } = [];
    public IEnumerable<int> RoleIds { get; init; } = [];
}

internal sealed class SaveApiEndpointRequest
{
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public IEnumerable<int> AppModuleIds { get; init; } = [];
    public IEnumerable<int> RoleIds { get; init; } = [];
}

// AppModule DTOs
internal sealed class AppModuleResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IEnumerable<int> RoleIds { get; init; } = [];
}

internal sealed class SaveAppModuleRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IEnumerable<int> RoleIds { get; init; } = [];
}

// Menu DTOs
internal sealed class MenuResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Header { get; init; } = string.Empty;
    public int Rank { get; init; }
    public int? ParentMenuId { get; init; }
    public IEnumerable<int> RoleIds { get; init; } = [];
}

internal sealed class SaveMenuRequest
{
    public string Name { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Header { get; init; } = string.Empty;
    public int Rank { get; init; }
    public int? ParentMenuId { get; init; }
    public IEnumerable<int> RoleIds { get; init; } = [];
}

// Role DTOs
internal sealed class RoleResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}

internal sealed class SaveRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}

// User DTOs
internal sealed class UserResponse
{
    public int Id { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

internal sealed class CreateUserRequest
{
    public string Provider { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IEnumerable<int> RoleIds { get; init; } = [];
}

internal sealed class UpdateUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Password { get; init; }
    public IEnumerable<int> RoleIds { get; init; } = [];
}