using DotBoil.EFCore;
using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Entities;
using DotBoil.Studio.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Studio.Core.Services;

public class MenuUISettingsService : IMenuUISettingsService
{
    private readonly IRepository<MenuUISettings, StudioDbContext> _repository;
    private readonly IAuditUser _auditUser;

    public MenuUISettingsService(
        IRepository<MenuUISettings, StudioDbContext> repository,
        IAuditUser auditUser)
    {
        _repository = repository;
        _auditUser = auditUser;
    }

    public IQueryable<MenuUISettings> GetAll() => _repository.Get();

    public async Task<MenuUISettings?> GetByIdAsync(int id)
        => await _repository.Get().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(MenuUISettings settings)
    {
        settings.CreateTime = DateTime.Now;
        settings.CreateUser = await _auditUser.GetModifierName();
        await _repository.AddAsync(settings);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuUISettings settings)
    {
        settings.UpdateTime = DateTime.Now;
        settings.ModifyUser = await _auditUser.GetModifierName();
        _repository.Update(settings);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(MenuUISettings settings)
    {
        _repository.Remove(settings);
        await _repository.SaveChangesAsync();
    }
}