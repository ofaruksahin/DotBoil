namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class RefreshTokenResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; set; }
    public string AccessToken { get; private set; }
    public DateTime? ExpireAccessToken { get; private set; }

    public RefreshTokenResult(
        bool isSuccess, 
        string message,
        string accessToken, 
        DateTime? expireAccessToken)
    {
        IsSuccess = isSuccess;
        Message = message;
        AccessToken = AccessToken;
        ExpireAccessToken = ExpireAccessToken;
    }
    
    public static RefreshTokenResult Success(string accessToken, DateTime expireAccessToken)
        => new RefreshTokenResult(true, string.Empty, accessToken, expireAccessToken);
    
    public static RefreshTokenResult Failure(string message)
        => new RefreshTokenResult(false, message, string.Empty, null);
}