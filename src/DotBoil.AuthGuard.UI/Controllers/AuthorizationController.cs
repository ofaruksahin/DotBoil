using DotBoil.AuthGuard.Application.Domain.Exceptions;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using DotBoil.AuthGuard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil.AuthGuard.Controllers;

public class AuthorizationController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IExternalSignInManager _externalSignInManager;

    public AuthorizationController(
        IConfiguration configuration,
        IUserService userService,
        IExternalSignInManager externalSignInManager)
    {
        _configuration = configuration;
        _userService = userService;
        _externalSignInManager = externalSignInManager;
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
        
        var authorizeResult = await _userService.SignIn(new AuthorizeRequest(viewModel.Email, viewModel.Password));

        if (authorizeResult.IsSuccess)
        {
            redirectUri =
                $"{redirectUri}?access_token={authorizeResult.AccessToken}&refresh_token={authorizeResult.RefreshToken}&expire_access_token={authorizeResult.ExpireAccessToken.ToString("dd/MM/yyyy-HH:mm")}&expire_refresh_token={authorizeResult.ExpireRefreshToken.ToString("dd/MM/yyyy-HH:mm")}";

            return Redirect(redirectUri);
        }
        
        ViewBag.Message = authorizeResult.Message;
        
        return View(viewModel);
    }

    [HttpGet("connect/{provider}")]
    public async Task<IActionResult> ExternalLogin(string provider)
    {
        var externalSignInManagers = _configuration.GetSection("DotBoil:ExternalSignInManagers").Get<List<ExternalSignInManagerViewModel>>() ?? new List<ExternalSignInManagerViewModel>();

        var externalSignInManager = externalSignInManagers.FirstOrDefault(e => e.ServiceName == provider);
        if (externalSignInManager is null)
            throw new ExternalSignInManagerNotSupportedException(provider);

        return Redirect(externalSignInManager.AuthorizationEndpoint);
    }

    [HttpGet("connect/external/{provider}")]
    public async Task<IActionResult> ExternalLoginCallback(string provider)
    {
        var code = HttpContext.Request.Query["code"].ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(code))
            throw new AuthorizationCodeEmptyException();
        
        var externalSignInManagers = _configuration.GetSection("DotBoil:ExternalSignInManagers").Get<List<ExternalSignInManagerViewModel>>() ?? new List<ExternalSignInManagerViewModel>();

        var externalSignInManager = externalSignInManagers.FirstOrDefault(e => e.ServiceName == provider);
        if (externalSignInManager is null)
            throw new ExternalSignInManagerNotSupportedException(provider);

        var tokenEndpoint = $"{externalSignInManager.TokenEndpoint}&code={code}";

        var externalLoginRequest = new ExternalLoginRequest(
            provider,
            externalSignInManager.TokenEndpoint,
            externalSignInManager.UserInfoEndpoint,
            externalSignInManager.EmailIdentifier,
            externalSignInManager.UsernameIdentifier,
            externalSignInManager.NameIdentifier,
            externalSignInManager.SurnameIdentifier);
        
        var authorizeResult = await _externalSignInManager.ExternalLogin(externalLoginRequest);

        if (!authorizeResult.IsSuccess)
            throw new ExternalSignInManagerAuthorizationException();

        var redirectUri = externalSignInManager.RedirectUrl;
        
        redirectUri =
            $"{redirectUri}?access_token={authorizeResult.AccessToken}&refresh_token={authorizeResult.RefreshToken}&expire_access_token={authorizeResult.ExpireAccessToken.ToString("dd/MM/yyyy-HH:mm")}&expire_refresh_token={authorizeResult.ExpireRefreshToken.ToString("dd/MM/yyyy-HH:mm")}";
        
        return Redirect(redirectUri);
    }
}