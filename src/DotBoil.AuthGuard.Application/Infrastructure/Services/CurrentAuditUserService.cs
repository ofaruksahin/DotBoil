using DotBoil.EFCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal class CurrentAuditUserService : IAuditUser
{
    public async Task<string> GetModifierName()
    {
        return "";
    }
}