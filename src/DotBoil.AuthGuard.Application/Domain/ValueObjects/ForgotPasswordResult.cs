namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class ForgotPasswordResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }

    public ForgotPasswordResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static ForgotPasswordResult Success(string message) => new ForgotPasswordResult(true, message);
    public static ForgotPasswordResult Fail(string message) => new ForgotPasswordResult(false, message);
}