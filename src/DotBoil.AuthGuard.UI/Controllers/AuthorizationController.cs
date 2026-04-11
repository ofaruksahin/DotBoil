using DotBoil.AuthGuard.Application.Domain.Exceptions;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.ViewModels;
using DotBoil.Localization;
using DotBoil.UserConsents;
using DotBoil.UserConsents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Controllers;

public class AuthorizationController : Controller
{
    private readonly IUserService _userService;
    private readonly ICurrentLanguage _currentLanguage;
    private readonly IUserConsentsService _userConsentsService;

    private static readonly ConsentType[] RegistrationConsentTypes =
    [
        ConsentType.TermsOfService,
        ConsentType.PrivacyPolicy,
        ConsentType.CookiePolicy
    ];

    public AuthorizationController(
        [FromKeyedServices("EmailPasswordBasedLogin")] IUserService userService,
        ICurrentLanguage currentLanguage,
        IUserConsentsService userConsentsService)
    {
        _userService = userService;
        _currentLanguage = currentLanguage;
        _userConsentsService = userConsentsService;
    }

    [HttpGet("/connect/authorize")]
    public IActionResult Authorize()
    {
        var redirectUri = HttpContext.Request.Query["redirect_uri"].ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(redirectUri))
            throw new InvalidRedirectUriException();

        var viewModel = new SignInViewModel();
        return View(viewModel);
    }

    [HttpPost("/connect/authorize")]
    public async Task<IActionResult> Authorize([FromForm] SignInViewModel viewModel)
    {
        var redirectUri = HttpContext.Request.Query["redirect_uri"].ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(redirectUri))
            throw new InvalidRedirectUriException();

        var authorizeParameters = new Dictionary<string, string>
        {
            { "Email", viewModel.Email },
            { "Password", viewModel.Password }
        };

        var authorizeResult = await _userService.SignIn(authorizeParameters);

        if (authorizeResult.IsSuccess)
        {
            var language = _currentLanguage.Language;

            redirectUri =
                $"{redirectUri}?access_token={authorizeResult.AccessToken}" +
                $"&refresh_token={authorizeResult.RefreshToken}" +
                $"&expire_access_token={authorizeResult.ExpireAccessToken.ToString("dd/MM/yyyy-HH:mm")}" +
                $"&expire_refresh_token={authorizeResult.ExpireRefreshToken.ToString("dd/MM/yyyy-HH:mm")}" +
                $"&language={language}";

            return Redirect(redirectUri);
        }

        ViewBag.MessageType = "danger";
        ViewBag.Message = authorizeResult.Message;

        return View(viewModel);
    }

    [HttpGet("authorize/register")]
    public async Task<IActionResult> Register()
    {
        await PopulateConsentsAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost("authorize/register")]
    public async Task<IActionResult> Register([FromForm] RegisterViewModel viewModel)
    {
        var language = _currentLanguage.Language;

        var signupParameters = new Dictionary<string, string>
        {
            { "Email", viewModel.Email },
            { "Password", viewModel.Password },
            { "Name", viewModel.Name },
            { "Surname", viewModel.Surname },
            { "Username", viewModel.Email },
            { "Language", language },
            { "AcceptedConsentTypes", string.Join(",", viewModel.AcceptedConsentTypes) }
        };

        var signupResult = await _userService.Signup(signupParameters);

        if (!signupResult.IsSuccess)
        {
            ViewBag.MessageType = "danger";
            ViewBag.Message = signupResult.Message;
            await PopulateConsentsAsync();
            return View(viewModel);
        }

        return RedirectToAction("Authorize", new { redirect_uri = HttpContext.Request.Query["redirect_uri"].ToString(), language });
    }

    [HttpGet("authorize/forgot_password")]
    public async Task<IActionResult> ForgotPassword()
    {
        return View();
    }

    [HttpPost("authorize/send_otp")]
    public async Task<IActionResult> SendOtp([FromForm] ForgotPasswordViewModel viewModel)
    {
        var parameters = new Dictionary<string, string>
        {
            { "Email", viewModel.Email }
        };

        var result = await _userService.SendForgotPasswordCode(parameters);

        if (!result.IsSuccess)
        {
            ViewBag.MessageType = "danger";
            ViewBag.Message = result.Message;
            return View("ForgotPassword");
        }

        return View("VerifyOtp", new VerifyOtpViewModel(viewModel.Email));
    }

    [HttpGet("authorize/resend_otp")]
    public async Task<IActionResult> ResendOtp([FromQuery] string email)
    {
        var parameters = new Dictionary<string, string>
        {
            { "Email", email }
        };

        var result = await _userService.ResendOtp(parameters);

        ViewBag.MessageType = result.IsSuccess ? "success" : "danger";
        ViewBag.Message = result.Message;
        return View("VerifyOtp", new VerifyOtpViewModel(email));
    }

    [HttpPost("authorize/verify_otp")]
    public async Task<IActionResult> VerifyOtp([FromForm] VerifyOtpViewModel viewModel)
    {
        var parameters = new Dictionary<string, string>
        {
            { "Email", viewModel.Email },
            { "OtpCode", viewModel.OtpCode },
            { "Password", viewModel.Password }
        };

        var result = await _userService.ForgotPassword(parameters);

        if (!result.IsSuccess)
        {
            ViewBag.MessageType = "danger";
            ViewBag.Message = result.Message;
            return View("VerifyOtp", new VerifyOtpViewModel(viewModel.Email));
        }

        var redirectUri = string.Empty;
        var language = _currentLanguage.Language;

        if (HttpContext.Request.Query.ContainsKey("redirect_uri"))
            redirectUri = HttpContext.Request.Query["redirect_uri"];

        return RedirectToAction("Authorize", new { redirect_uri = redirectUri, language });
    }

    private async Task PopulateConsentsAsync()
    {
        var language = _currentLanguage.Language;
        var consentTasks = RegistrationConsentTypes.Select(t => _userConsentsService.GetConsent(t, language));
        var results = await Task.WhenAll(consentTasks);
        ViewBag.Consents = results.Where(c => c != null).ToList();
    }
}