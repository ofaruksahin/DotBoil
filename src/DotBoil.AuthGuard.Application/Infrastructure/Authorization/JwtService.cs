using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotBoil.AuthGuard.Application.Infrastructure.Authorization;

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;

    public JwtService(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }
    
    public string GenerateToken(IEnumerable<Claim> claims, int expirationMinutes)
    {
        var secretKey = _jwtOptions.SecretKey;
        var issuer = _jwtOptions.Issuer;
        var audience = _jwtOptions.Audience;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}