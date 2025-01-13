namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class ExternalLoginRequest
{
    public string Provider { get; private set; }
    public string TokenEndpoint { get; private set; }
    public string UserInfoEndpoint { get; private set; }
    public string EmailIdentifier { get; private set; }
    public string UsernameIdentifier { get; private set; }
    public string NameIdentifier { get; private set; }
    public string SurnameIdentifier { get; private set; }

    public ExternalLoginRequest(
        string provider,
        string tokenEndpoint,
        string userInfoEndpoint,
        string emailIdentifier,
        string usernameIdentifier,
        string nameIdentifier,
        string surnameIdentifier)
    {
        Provider = provider;
        TokenEndpoint = tokenEndpoint;
        UserInfoEndpoint = userInfoEndpoint;
        EmailIdentifier = emailIdentifier;
        UsernameIdentifier = usernameIdentifier;
        NameIdentifier = nameIdentifier;
        SurnameIdentifier = surnameIdentifier;
    }
}