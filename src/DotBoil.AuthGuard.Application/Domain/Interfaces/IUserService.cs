using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using Mysqlx.Datatypes;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IUserService
{
    Task<AuthorizeResult> SignIn(AuthorizeRequest authorizeRequest);
    Task<SignupResult> Signup(SignupRequest signupRequest);
    Task<ForgotPasswordResult> ForgotPassword(string email);
    Task<ForgotPasswordResult> ForgotPassword(string otpCode, string password);
    Task<ResendOtpResult> ResendOtp(string email);
}