using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.MessageBroker;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal enum OtpCodeVerificationResult
{
    Valid,
    Invalid,
    Expired
}

internal static class OtpCodeRepositoryExtensions
{
    private const int OtpCodeExpirationMinutes = 10;

    // Expires the user's open codes for the same purpose, then creates a new one.
    // The event created by eventFactory is published when the code is saved.
    public static async Task<OtpCode> CreateOtpCode(
        this IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        int userId,
        int purpose,
        Func<string, IEvent> eventFactory)
    {
        await otpCodeRepository
            .Get()
            .Where(oc => oc.UserId == userId && oc.Purpose == purpose && !oc.IsUsed && !oc.IsExpired)
            .ExecuteUpdateAsync(oc => oc.SetProperty(p => p.IsExpired, true));

        var otpCode = new OtpCode
        {
            UserId = userId,
            Purpose = purpose,
            ExpiryDate = DateTime.Now.AddMinutes(OtpCodeExpirationMinutes)
        };

        otpCode.GenerateOtpCode();
        otpCode.AddEvent(eventFactory(otpCode.Code));

        await otpCodeRepository.AddAsync(otpCode);
        await otpCodeRepository.SaveChangesAsync();

        return otpCode;
    }

    // Marks the code as used when it is valid, or as expired when its time has passed.
    public static async Task<OtpCodeVerificationResult> VerifyOtpCode(
        this IRepository<OtpCode, DotBoilAuthGuardDbContext> otpCodeRepository,
        int userId,
        int purpose,
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return OtpCodeVerificationResult.Invalid;

        var otpCode = await otpCodeRepository
            .GetAll()
            .AsTracking()
            .FirstOrDefaultAsync(oc =>
                oc.UserId == userId &&
                oc.Purpose == purpose &&
                oc.Code == code &&
                !oc.IsUsed &&
                !oc.IsExpired);

        if (otpCode is null)
            return OtpCodeVerificationResult.Invalid;

        if (otpCode.ExpiryDate < DateTime.Now)
        {
            otpCode.IsExpired = true;
            otpCodeRepository.Update(otpCode);
            await otpCodeRepository.SaveChangesAsync();

            return OtpCodeVerificationResult.Expired;
        }

        otpCode.IsUsed = true;
        otpCodeRepository.Update(otpCode);
        await otpCodeRepository.SaveChangesAsync();

        return OtpCodeVerificationResult.Valid;
    }
}
