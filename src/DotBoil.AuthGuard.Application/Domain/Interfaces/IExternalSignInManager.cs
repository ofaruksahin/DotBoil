using DotBoil.AuthGuard.Application.Domain.ValueObjects;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IExternalSignInManager
{
    Task<AuthorizeResult> ExternalLogin(ExternalLoginRequest externalLoginRequest);
}