namespace DotBoil.AuthGuard.Application.Domain.Exceptions;

public class InvalidRedirectUriException : Exception
{
    public InvalidRedirectUriException() : base("Invalid redirect URI")
    {
        
    }
}