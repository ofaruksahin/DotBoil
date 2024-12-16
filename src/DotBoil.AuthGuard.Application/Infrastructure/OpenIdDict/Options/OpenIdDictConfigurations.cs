using DotBoil.Configuration;

namespace DotBoil.AuthGuard.Application.Infrastructure.OpenIdDict.Options;

public class OpenIdDictConfigurations : IOptions
{
    public string Key => "DotBoil:AuthGuard:OpenIdDict";
    public OpenIdDictCoreConfiguration Core { get; set; }
    public OpenIdDictClientConfiguration Client { get; set; }
    public OpenIdDictServerConfiguration Server { get; set; }
}

public class OpenIdDictCoreConfiguration
{
    public string ConnectionString { get; set; }
}

public class OpenIdDictClientConfiguration
{
    public bool EnableAuthorizationCodeFlow { get; set; }
    public bool EnableClientCredentialsFlow { get; set; }
    public bool EnableRefreshTokenFlow { get; set; }
    public bool EnableDeviceCodeFlow { get; set; }
}

public class OpenIdDictServerConfiguration
{
    public string[] AuthorizationEndpointUris { get; set; } = Array.Empty<string>();
    public string[] LogoutEndpointUris { get; set; } = Array.Empty<string>();
    public string[] TokenEndpointUris { get; set; } = Array.Empty<string>();
    public string[] UserInfoEndpointUris { get; set; } = Array.Empty<string>();
    public string[] RevocationEndpointUris { get; set; } = Array.Empty<string>();
    public string[] DeviceEndpointUris { get; set; } = Array.Empty<string>();
    public bool EnableAuthorizationCodeFlow { get; set; }
    public bool EnableClientCredentialsFlow { get; set; }
    public bool EnableRefreshTokenFlow { get; set; }
    public int? AccessTokenLifetimeMinute { get; set; }
    public int? AuthorizationCodeLifetimeMinute { get; set; }
    public int? DeviceCodeLifetimeMinute{ get; set; }
    public int? IdentityTokenLifetimeMinute { get; set; }
    public int? RefreshTokenLifetimeMinute { get; set; }
    public int? UserCodeLifetimeMinute { get; set; }

    public OpenIdDictApplicationConfiguration[] Applications { get; set; } =
        Array.Empty<OpenIdDictApplicationConfiguration>();

    public TimeSpan AccessTokenLifetime => AccessTokenLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(AccessTokenLifetimeMinute.Value)
        : TimeSpan.Zero;

    public TimeSpan AuthorizationCodeLifetime => AuthorizationCodeLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(AuthorizationCodeLifetimeMinute.Value)
        : TimeSpan.Zero;

    public TimeSpan DeviceCodeLifetime => DeviceCodeLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(DeviceCodeLifetimeMinute.Value)
        : TimeSpan.Zero;

    public TimeSpan IdentityTokenLifetime => IdentityTokenLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(IdentityTokenLifetimeMinute.Value)
        : TimeSpan.Zero;
    
    public TimeSpan RefreshTokenLifetime => RefreshTokenLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(RefreshTokenLifetimeMinute.Value)
        : TimeSpan.Zero;

    public TimeSpan UserCodeLifetime => UserCodeLifetimeMinute.HasValue
        ? TimeSpan.FromMinutes(UserCodeLifetimeMinute.Value)
        : TimeSpan.Zero;
}

public class OpenIdDictApplicationConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string DisplayName { get; set; }
    public string[] RedirectUris { get; set; } = Array.Empty<string>();
    public string[] PostLogoutRedirectUris { get; set; } = Array.Empty<string>();
}