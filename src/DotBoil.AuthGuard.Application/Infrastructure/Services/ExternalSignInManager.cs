using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Nodes;
using DotBoil.AuthGuard.Application.Domain.DomainEvents;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class ExternalSignInManager : IExternalSignInManager
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly IJwtService _jwtService;
    private readonly ICache _cache;
    private readonly ILocalize _localize;

    public ExternalSignInManager(
        HttpClient httpClient,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        JwtOptions jwtOptions,
        IJwtService jwtService,
        ICache cache,
        ILocalize localize)
    {
        _httpClient = httpClient;
        _userRepository = userRepository;
        _jwtOptions = jwtOptions;
        _jwtService = jwtService;
        _cache = cache;
        _localize = localize;
    }
    
    public async Task<AuthorizeResult> ExternalLogin(ExternalLoginRequest externalLoginRequest)
    {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var tokenResponse = await _httpClient.PostAsync(new Uri(externalLoginRequest.TokenEndpoint), null);

        if (!tokenResponse.IsSuccessStatusCode)
            return AuthorizeResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        var tokenResponseBody = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var accessToken = tokenResponseBody.AccessToken;
        
        if (string.IsNullOrEmpty(accessToken))
            return AuthorizeResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DotBoil.AuthGuard.Application", "1.0"));
        var userInfoResponse = await _httpClient.GetAsync(new Uri(externalLoginRequest.UserInfoEndpoint));
        
        if (!userInfoResponse.IsSuccessStatusCode)
            return AuthorizeResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        var userInfoResponseMessage = await userInfoResponse.Content.ReadAsStringAsync();
        var jObject =  JsonObject.Parse(userInfoResponseMessage);
        var emailIdentifier = string.Empty;
        var usernameIdentifier = string.Empty;
        var nameIdentifier = string.Empty;
        var surnameIdentifier = string.Empty;

        if (!string.IsNullOrEmpty(externalLoginRequest.EmailIdentifier))
            emailIdentifier = jObject[externalLoginRequest.EmailIdentifier].ToString();
        
        if (!string.IsNullOrEmpty(externalLoginRequest.UsernameIdentifier))
            usernameIdentifier = jObject[externalLoginRequest.UsernameIdentifier].ToString();
        
        if (!string.IsNullOrEmpty(externalLoginRequest.NameIdentifier))
            nameIdentifier = jObject[externalLoginRequest.NameIdentifier].ToString();
        
        if (!string.IsNullOrEmpty(externalLoginRequest.SurnameIdentifier))
            surnameIdentifier = jObject[externalLoginRequest.SurnameIdentifier].ToString();

        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Email == emailIdentifier);

        if (user is null)
        {
            user = new User
            {
                Provider = externalLoginRequest.Provider,
                Name = nameIdentifier,
                Surname = surnameIdentifier,
                Username = usernameIdentifier,
                Email = emailIdentifier,
                Password = string.Empty
            };

            var userCreatedEvent = new UserCreatedDomainEvent(
                user.Email,
                user.Name,
                user.Surname,
                user.Username);
            
            user.AddEvent(userCreatedEvent);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.Surname),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var claimsDictionary = claims.ToDictionary(claim => claim.Type, claim => claim.Value);

        var createdAccessToken = await _jwtService.GenerateToken(claims, _jwtOptions.AccessTokenExpirationMinutes);
        var createdRefreshToken = await _jwtService.GenerateToken(claims, _jwtOptions.RefreshTokenExpirationMinutes);
        var createdAccessTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var createdRefreshTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes);
        
        await _cache.SetAsync($"DotBoil:AuthGuard:AccessTokens:{createdAccessToken}", claimsDictionary, TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));
        await _cache.SetAsync($"DotBoil:AuthGuard:RefreshTokens:{createdRefreshToken}", claimsDictionary, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpirationMinutes));
        
        return AuthorizeResult.Success(
            createdAccessToken,
            createdRefreshToken,
            createdAccessTokenExpiryTime, 
            createdRefreshTokenExpiryTime);
    }
}
