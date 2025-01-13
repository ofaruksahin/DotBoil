namespace DotBoil.AuthGuard.Application.Domain.Exceptions;

public class ExternalSignInManagerAuthorizationException : Exception
{
    public ExternalSignInManagerAuthorizationException() : base("External Sign In Manager Authorization Failed.")
    {
        
    }
}