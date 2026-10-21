namespace DotBoil.AuthGuard.Application.Domain.Entities;

// Built-in OTP purposes. Purpose is stored as int so applications can define their own values;
// custom purposes should start from 1000 to avoid clashing with future built-in ones.
public static class OtpCodePurpose
{
    public const int ForgotPassword = 1;
    public const int AccountDeletion = 2;
    public const int AccountActivation = 3;
}
