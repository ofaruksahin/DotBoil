namespace DotBoil.AuthGuard.Application.Domain.Exceptions;

public class ExternalSignInManagerNotSupportedException : Exception
{
    public ExternalSignInManagerNotSupportedException(string provider) : base($"{provider} External Signin manager is not supported")
    {
        
    }
}