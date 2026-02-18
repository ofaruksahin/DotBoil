using DotBoil.AuthGuard.Application.Domain.ValueObjects;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IUserService
{
    Task<AuthorizeResult> SignIn(Dictionary<string, string> parameters);
    Task<SignupResult> Signup(Dictionary<string, string> parameters);
    Task<ForgotPasswordResult> SendForgotPasswordCode(Dictionary<string, string> parameters);
    Task<ForgotPasswordResult> ForgotPassword(Dictionary<string, string> parameters);
    Task<ResendOtpResult> ResendOtp(Dictionary<string, string> parameters);
    Task<RefreshTokenResult> RefreshToken(string refreshToken);
    Task<GetUserInfoResponse> GetUserInfo();
}
