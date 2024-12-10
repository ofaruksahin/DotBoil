using DotBoil.EFCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class CurrentAuditUserService : IAuditUser
{
    public async Task<string> GetModifierName()
    {
        return "";
    }
}