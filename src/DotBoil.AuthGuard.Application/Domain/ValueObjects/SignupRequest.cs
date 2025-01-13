namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class SignupRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    public SignupRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }
}