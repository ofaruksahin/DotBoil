namespace DotBoil.AuthGuard.Application.Domain.Exceptions;

public class AuthorizationCodeEmptyException : Exception
{
    public AuthorizationCodeEmptyException() : base("Authorization code is empty")
    {

    }
}