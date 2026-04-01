using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.DomainEvents;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class EmailPasswordBasedLogin : IUserService
{
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly IRepository<OtpCode, DotBoilAuthGuardDbContext> _otpCodeRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;
    private readonly ICache _cache;
    private readonly ILocalize _localize;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailPasswordBasedLogin(
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        IJwtService jwtService,
        JwtOptions jwtOptions,
        ICache cache,
        ILocalize localize,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _jwtService = jwtService;
        _jwtOptions = jwtOptions;
        _cache = cache;
        _localize = localize;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<AuthorizeResult> SignIn(Dictionary<string, string> parameters)
    {
        var email = parameters.GetValueOrDefault("Email") ?? string.Empty;
        var password = parameters.GetValueOrDefault("Password") ?? string.Empty;

        var encryptedPassword = EncryptProvider.Md5(password);
        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.Password == encryptedPassword &&
                u.Provider == string.Empty);

        if (user is null)
            return AuthorizeResult.Failure(await _localize.LocalizeText("Login", "InvalidEmailOrPassword"));
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.Surname),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);
        
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

    public async Task<SignupResult> Signup(Dictionary<string, string> parameters)
    {
        var email = parameters.GetValueOrDefault("Email") ?? string.Empty;
        var password = parameters.GetValueOrDefault("Password") ?? string.Empty;
        var name = parameters.GetValueOrDefault("Name") ?? string.Empty;
        var surname = parameters.GetValueOrDefault("Surname") ?? string.Empty;
        var username = parameters.GetValueOrDefault("Username") ?? email;

        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
            return SignupResult.Failure(await _localize.LocalizeText("Login", "UserAlreadyExists"));

        user = new User
        {
            Provider = string.Empty,
            Name = name,
            Surname = surname,
            Username = username,
            Email = email,
            Password = EncryptProvider.Md5(password)
        };

        var userCreatedEvent = new UserCreatedDomainEvent(user.Email, user.Name, user.Surname, user.Username);
        
        user.AddEvent(userCreatedEvent);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return SignupResult.Success(await _localize.LocalizeText("Success"));
    }

    public async Task<ForgotPasswordResult> SendForgotPasswordCode(Dictionary<string, string> parameters)
    {
        var email = parameters.GetValueOrDefault("Email") ?? string.Empty;

        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Provider == string.Empty && u.Email == email);
        
        if (user is null)
            return ForgotPasswordResult.Fail(await _localize.LocalizeText("Login", "UserNotFound"));
        
        await _otpCodeRepository
            .Get()
            .Where(oc => oc.UserId == user.Id && (!oc.IsUsed && !oc.IsExpired))
            .ExecuteUpdateAsync(oc => oc.SetProperty(p => p.IsExpired, true));

        await _otpCodeRepository.SaveChangesAsync();

        var otpCode = new OtpCode
        {
            UserId = user.Id,
            ExpiryDate = DateTime.Now.AddMinutes(10)
        };
        
        otpCode.GenerateOtpCode();

        var forgotPasswordOtpCodeCreatedDomainEvent =
            new ForgotPasswordOtpCodeCreatedDomainEvent(user.Email, otpCode.Code);
        
        otpCode.AddEvent(forgotPasswordOtpCodeCreatedDomainEvent);
        
        await _otpCodeRepository.AddAsync(otpCode);
        await _otpCodeRepository.SaveChangesAsync();
        
        return ForgotPasswordResult.Success(await _localize.LocalizeText("Success"));
    }

    public async Task<ForgotPasswordResult> ForgotPassword(Dictionary<string, string> parameters)
    {
        var otpCode = parameters.GetValueOrDefault("OtpCode") ?? string.Empty;
        var password = parameters.GetValueOrDefault("Password") ?? string.Empty;

        var otp = await _otpCodeRepository
            .Get()
            .Include(oc => oc.User)
            .FirstOrDefaultAsync(oc => !oc.IsExpired && !oc.IsUsed && oc.Code == otpCode);
        
        if (otp is null)
            return ForgotPasswordResult.Fail(await _localize.LocalizeText("OtpCodeInvalid"));

        if (otp.ExpiryDate < DateTime.Now)
        {
            otp.IsExpired = true;
            _otpCodeRepository.Update(otp);
            await _otpCodeRepository.SaveChangesAsync();
            
            return ForgotPasswordResult.Fail(await _localize.LocalizeText("OtpCodeExpired"));
        }

        otp.IsUsed = true;
        otp.User.Password = EncryptProvider.Md5(password);
        
        _otpCodeRepository.Update(otp);
        await _otpCodeRepository.SaveChangesAsync();
        
        return ForgotPasswordResult.Success(await _localize.LocalizeText("Success"));
    }

    public async Task<ResendOtpResult> ResendOtp(Dictionary<string, string> parameters)
    {
        var result = await SendForgotPasswordCode(parameters);

        return new ResendOtpResult(result.IsSuccess, result.Message);
    }

    public async Task<GetUserInfoResponse> GetUserInfo()
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims;

        if (claims is null || !claims.Any())
        {
            return new GetUserInfoResponse(false, string.Empty, new Dictionary<string, string>());
        }
        
        return new GetUserInfoResponse(true, string.Empty, claims.ToDictionary(c => c.Type, c => c.Value));
    }
}
