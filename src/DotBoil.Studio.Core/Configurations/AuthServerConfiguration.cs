using DotBoil.Configuration;

namespace DotBoil.Studio.Core.Configurations;

internal class AuthServerConfiguration : IOptions
{
    public string Key => "DotBoil:Studio:AuthServer";

    public string Url { get; set; }
    public string AuthorizeEndpoint { get; set; }
    public string RefreshTokenEndpoint { get; set; }
    public string CallbackEndpoint { get; set; }
    public string MenuEndpoint { get; set; }
    public string TokenKey { get; set; }
    public string RefreshTokenKey { get; set; }
}