using DotBoil.AuthGuard.Application.Domain.ValueObjects;

namespace DotBoil.AuthGuard.Application.Domain.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<MenuItem>> GetMenuItems();
}