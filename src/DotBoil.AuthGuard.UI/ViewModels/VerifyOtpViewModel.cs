namespace DotBoil.AuthGuard.ViewModels;

public class VerifyOtpViewModel
{
    public string Email { get; set; }
    public string OtpCode { get; set; }
    public string Password { get; set; }

    public VerifyOtpViewModel()
    {
    }

    public VerifyOtpViewModel(string email)
    {
        Email = email;
    }
}