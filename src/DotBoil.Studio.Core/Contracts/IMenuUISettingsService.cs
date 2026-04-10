using DotBoil.Studio.Core.Entities;

namespace DotBoil.Studio.Core.Contracts;

public interface IMenuUISettingsService
{
    IQueryable<MenuUISettings> GetAll();
    Task<MenuUISettings?> GetByIdAsync(int id);
    Task CreateAsync(MenuUISettings settings);
    Task UpdateAsync(MenuUISettings settings);
    Task DeleteAsync(MenuUISettings settings);
}