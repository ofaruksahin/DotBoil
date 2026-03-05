namespace DotBoil.Studio.Core.Models;

public class RefreshTokenResult
{
    public bool IsSuccess { get;  set; }
    public string Message { get; set; }
    public string AccessToken { get;  set; }
    public DateTime? ExpireAccessToken { get;  set; }
}