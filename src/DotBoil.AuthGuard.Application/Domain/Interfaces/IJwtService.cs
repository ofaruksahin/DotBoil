using System.Security.Claims;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(IEnumerable<Claim> claims, int expirationMinutes);
}