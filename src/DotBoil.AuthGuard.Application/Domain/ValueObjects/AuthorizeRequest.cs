namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class AuthorizeRequest
{
    public string Email { get; private set; }
    public string Password { get; private set; }

    public AuthorizeRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }
}