using System.Security.Claims;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IJwtService
{
    Task<string> GenerateToken(IEnumerable<Claim> claims, int expirationMinutes);
}
