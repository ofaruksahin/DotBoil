namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IPermissionService
{
    Task<bool> CheckPermission(string controller, string action);
}