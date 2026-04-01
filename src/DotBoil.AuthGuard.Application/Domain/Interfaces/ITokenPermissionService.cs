using DotBoil.AuthGuard.Application.Domain.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

internal interface ITokenPermissionService
{
    Task<IEnumerable<Role>> GetCurrentUserRoles(int userId);
    Task<IEnumerable<AppModule>> GetCurrentUserAppModules(int userId);
}
