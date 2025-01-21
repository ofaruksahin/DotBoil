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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NETCore.Encrypt;
using ZstdSharp.Unsafe;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly IRepository<OtpCode, DotBoilAuthGuardDbContext> _otpCodeRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;
    private readonly ICache _cache;
    private readonly ILocalize _localize;

    public UserService(
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        IJwtService jwtService,
        JwtOptions jwtOptions,
        ICache cache,
        ILocalize localize)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _jwtService = jwtService;
        _jwtOptions = jwtOptions;
        _cache = cache;
        _localize = localize;
    }
    
    public async Task<AuthorizeResult> SignIn(AuthorizeRequest authorizeRequest)
    {
        var encryptedPassword = EncryptProvider.Md5(authorizeRequest.Password);
        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u =>
                    u.Email == authorizeRequest.Email &&
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
        
        var createdAccessToken = _jwtService.GenerateToken(claims, _jwtOptions.AccessTokenExpirationMinutes);
        var createdRefreshToken = _jwtService.GenerateToken(claims, _jwtOptions.RefreshTokenExpirationMinutes);
        var createdAccessTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var createdRefreshTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes);
        
        await _cache.SetAsync($"DotBoil:AuthGuard:AccessTokens:{createdAccessToken}", claims, TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));
        await _cache.SetAsync($"DotBoil:AuthGuard:RefreshTokens:{createdRefreshToken}", claims, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpirationMinutes));
        
        return AuthorizeResult.Success(
            createdAccessToken,
            createdRefreshToken,
            createdAccessTokenExpiryTime, 
            createdRefreshTokenExpiryTime);
    }

    public async Task<SignupResult> Signup(SignupRequest signupRequest)
    {
        var user = await _userRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Email == signupRequest.Email);

        if (user != null)
            return SignupResult.Failure(await _localize.LocalizeText("Login", "UserAlreadyExists"));

        user = new User
        {
            Provider = string.Empty,
            Name = signupRequest.Name,
            Surname = signupRequest.Surname,
            Username = signupRequest.Username,
            Email = signupRequest.Email,
            Password = EncryptProvider.Md5(signupRequest.Password)
        };

        var userCreatedEvent = new UserCreatedDomainEvent(user.Email, user.Name, user.Surname, user.Username);
        
        user.AddEvent(userCreatedEvent);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return SignupResult.Success(await _localize.LocalizeText("Success"));
    }
    public async Task<ForgotPasswordResult> ForgotPassword(string email)
    {
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

        var otpCode = new OtpCode()
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

    public async Task<ForgotPasswordResult> ForgotPassword(string otpCode, string password)
    {
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

    public async Task<ResendOtpResult> ResendOtp(string email)
    {
        var result = await ForgotPassword(email);

        return new ResendOtpResult(result.IsSuccess, result.Message);
    }

    public async Task<RefreshTokenResult> RefreshToken(string refreshToken)
    {
        var refreshTokenExists = await _cache.KeyExistsAsync($"DotBoil:AuthGuard:RefreshTokens:{refreshToken}");
        
        if (!refreshTokenExists)
            return RefreshTokenResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));
        
        var claims = await _cache.GetOrSetAsync(refreshToken, async () => { return new List<Claim>();}, TimeSpan.FromSeconds(5));
        
        if (!claims.Any())
            return RefreshTokenResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        var createdAccessToken =
            _jwtService.GenerateToken(claims, _jwtOptions.AccessTokenExpirationMinutes);
        var createdAccessTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        
        await _cache.SetAsync($"DotBoil:AuthGuard:AccessTokens:{createdAccessToken}", claims, TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));
        
        return RefreshTokenResult.Success(createdAccessToken, createdAccessTokenExpiryTime);
    }
}