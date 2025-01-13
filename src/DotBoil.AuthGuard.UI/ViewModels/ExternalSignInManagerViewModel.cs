namespace DotBoil.AuthGuard.ViewModels;

public class ExternalSignInManagerViewModel
{
    public string Logo { get; set; }
    public string ServiceName { get; set; }
    public string AuthorizationEndpoint { get; set; }
    public string TokenEndpoint { get; set; }
    public string UserInfoEndpoint { get; set; }
    public string EmailIdentifier { get; set; }
    public string UsernameIdentifier { get; set; }
    public string NameIdentifier { get; set; }
    public string SurnameIdentifier { get; set; }
    public string RedirectUrl { get; set; }
}