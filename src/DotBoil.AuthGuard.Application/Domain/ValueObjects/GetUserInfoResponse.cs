namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class GetUserInfoResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public IDictionary<string, string> Claims { get; set; }

    public GetUserInfoResponse()
    {
        Claims = new Dictionary<string, string>();
    }

    public GetUserInfoResponse(bool isSuccess, string message, IDictionary<string, string> claims)
    {
        IsSuccess = isSuccess;
        Message = message;
        Claims = claims;
    }
}