using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Entities;

namespace DotBoil.Studio.Core.Services;

public interface IUIConfigService
{
    IQueryable<UIConfig> GetAll();
    Task<UIConfig> GetByIdAsync(int id);
    Task CreateAsync(UIConfig config);
    Task UpdateAsync(UIConfig config);
    Task DeleteAsync(UIConfig config);
    IQueryable<UIConfigVersion> GetVersions(int uiConfigId);
    string SerializeComponents(List<BaseComponent> components);
    List<BaseComponent> DeserializeComponents(string? stateData);
}