namespace DotBoil.Studio.Core.ValueObjects;

public class RefreshTokenResult
{
    public bool IsSuccess { get;  set; }
    public string Message { get; set; }
    public string AccessToken { get;  set; }
    public DateTime? ExpireAccessToken { get;  set; }
}