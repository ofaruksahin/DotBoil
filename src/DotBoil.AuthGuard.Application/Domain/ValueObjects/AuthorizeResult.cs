using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class AuthorizeResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpireAccessToken { get; set; }
    public DateTime ExpireRefreshToken { get; set; }

    public static AuthorizeResult Success(string accessToken, string refreshToken, DateTime expireAccessToken, DateTime expireRefreshToken)
    {
        return new AuthorizeResult
        {
            IsSuccess = true,
            Message = string.Empty,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpireAccessToken = expireAccessToken,
            ExpireRefreshToken = expireRefreshToken
        };
    }

    public static AuthorizeResult Success(string message, string accessToken, string refreshToken, DateTime expireAccessToken, DateTime expireRefreshToken)
    {
        return new AuthorizeResult
        {
            IsSuccess = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpireAccessToken = expireAccessToken,
            ExpireRefreshToken = expireRefreshToken
        };
    }

    public static AuthorizeResult Failure(string message)
    {
        return new AuthorizeResult
        {
            IsSuccess = false,
            Message = message,
            AccessToken = string.Empty,
            RefreshToken = string.Empty
        };
    }
}