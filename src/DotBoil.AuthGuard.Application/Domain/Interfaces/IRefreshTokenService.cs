using DotBoil.AuthGuard.Application.Domain.ValueObjects;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

internal interface IRefreshTokenService
{
    Task<RefreshTokenResult> RefreshToken(string refreshToken);
}
