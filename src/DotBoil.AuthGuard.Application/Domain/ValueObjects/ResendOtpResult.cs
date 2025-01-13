namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class ResendOtpResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }

    public ResendOtpResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
    
    public static ResendOtpResult Success(string message) => new ResendOtpResult(true, message);
    public static ResendOtpResult Failed(string message) => new ResendOtpResult(false, message);
}