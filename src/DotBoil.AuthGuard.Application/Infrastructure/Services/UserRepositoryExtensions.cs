using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal static class UserRepositoryExtensions
{
    public static Task UpdateLastLoginDate(this IRepository<User, DotBoilAuthGuardDbContext> userRepository, int userId)
    {
        return userRepository
            .Get()
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.LastLoginDate, DateTime.Now));
    }
}
