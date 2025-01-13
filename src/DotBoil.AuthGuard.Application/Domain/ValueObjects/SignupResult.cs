namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

public class SignupResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }

    public SignupResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
    
    public static SignupResult Success() => new SignupResult(true, string.Empty);
    public static SignupResult Success(string message) => new SignupResult(true, message);
    public static SignupResult Failure(string message) => new SignupResult(false, message);
}