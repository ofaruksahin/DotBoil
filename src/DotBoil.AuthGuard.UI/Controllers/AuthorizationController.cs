using DotBoil.AuthGuard.Application.Domain.Exceptions;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Controllers;

public class AuthorizationController : Controller
{
    private readonly IUserService _userService;

    public AuthorizationController(
        [FromKeyedServices("EmailPasswordBasedLogin")] IUserService userService)
    {
        _userService = userService;
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
    public async Task<IActionResult> Authorize([FromForm]SignInViewModel viewModel)
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
            redirectUri =
                $"{redirectUri}?access_token={authorizeResult.AccessToken}&refresh_token={authorizeResult.RefreshToken}&expire_access_token={authorizeResult.ExpireAccessToken.ToString("dd/MM/yyyy-HH:mm")}&expire_refresh_token={authorizeResult.ExpireRefreshToken.ToString("dd/MM/yyyy-HH:mm")}";

            return Redirect(redirectUri);
        }
        
        ViewBag.Message = authorizeResult.Message;
        
        return View(viewModel);
    }

    [HttpGet("authorize/register")]
    public async Task<IActionResult> Register()
    {
        return View();
    }

    [HttpPost("authorize/register")]
    public async Task<IActionResult> Register([FromForm] RegisterViewModel viewModel)
    {
        var signupParameters = new Dictionary<string, string>
        {
            { "Email", viewModel.Email },
            { "Password", viewModel.Password },
            { "Name", viewModel.Name },
            { "Surname", viewModel.Surname },
            { "Username", viewModel.Email }
        };

        var signupResult = await _userService.Signup(signupParameters);

        ViewBag.Message = signupResult.Message;
        
        return View(viewModel);
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

        var result =  await _userService.SendForgotPasswordCode(parameters);

        if (!result.IsSuccess)
        {
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
            ViewBag.Message = result.Message;
            return View("VerifyOtp", new VerifyOtpViewModel(viewModel.Email));
        }

        var redirectUri = string.Empty;
        var language = "TR";
        
        if (HttpContext.Request.Query.ContainsKey("redirect_uri"))
            redirectUri = HttpContext.Request.Query["redirect_uri"];
        
        if (HttpContext.Request.Query.ContainsKey("language"))
            language = HttpContext.Request.Query["language"];
        
        return RedirectToAction("Authorize", new { redirect_uri = redirectUri, language = language });
    }
}
