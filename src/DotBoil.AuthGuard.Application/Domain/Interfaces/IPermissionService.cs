using DotBoil.AuthGuard.Application.Domain.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IPermissionService
{
    Task<bool> CheckPermission(string controller, string action);
    Task<IEnumerable<Role>> GetCurrentUserRoles();
    Task<IEnumerable<AppModule>> GetCurrentUserAppModules();
}
