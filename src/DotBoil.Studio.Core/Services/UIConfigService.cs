using System.Text.Json;
using DotBoil.EFCore;
using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Entities;
using DotBoil.Studio.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Studio.Core.Services;

public class UIConfigService : IUIConfigService
{
    private readonly IRepository<UIConfig, StudioDbContext> _repository;
    private readonly IRepository<UIConfigVersion, StudioDbContext> _versionRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters =
        {
            new PolymorphicJsonConverter<BaseComponent>(),
            new PolymorphicJsonConverter<DataSource>(),
            new HttpMethodJsonConverter(),
            new TypeJsonConverter()
        }
    };

    public UIConfigService(
        IRepository<UIConfig, StudioDbContext> repository,
        IRepository<UIConfigVersion, StudioDbContext> versionRepository)
    {
        _repository = repository;
        _versionRepository = versionRepository;
    }

    public IQueryable<UIConfig> GetAll() => _repository.Get();

    public async Task<UIConfig> GetByIdAsync(int id) => await _repository.Get().Where(i => i.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(UIConfig config)
    {
        config.CreateTime = DateTime.Now;
        config.CreateUser = "SYSTEM";
        await _repository.AddAsync(config);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(UIConfig config)
    {
        // Snapshot current state before applying changes
        var current = await _repository.Get().Where(c => c.Id == config.Id).FirstOrDefaultAsync();

        var nextVersion = (await _versionRepository.Get()
            .Where(v => v.UIConfigId == config.Id)
            .Select(v => (int?)v.Version)
            .MaxAsync() ?? 0) + 1;

        await _versionRepository.AddAsync(new UIConfigVersion
        {
            UIConfigId = config.Id,
            Version = nextVersion,
            Name = current.Name,
            Description = current.Description,
            StateData = current.StateData,
            CreateTime = DateTime.Now,
            CreateUser = "SYSTEM"
        });

        config.UpdateTime = DateTime.Now;
        config.ModifyUser = "SYSTEM";
        _repository.Update(config);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(UIConfig config)
    {
        _repository.Remove(config);
        await _repository.SaveChangesAsync();
    }

    public IQueryable<UIConfigVersion> GetVersions(int uiConfigId)
        => _versionRepository.Get()
            .Where(v => v.UIConfigId == uiConfigId)
            .OrderByDescending(v => v.Version);

    public string SerializeComponents(List<BaseComponent> components)
        => JsonSerializer.Serialize(components, JsonOptions);

    public List<BaseComponent> DeserializeComponents(string? stateData)
    {
        if (string.IsNullOrWhiteSpace(stateData)) return [];
        return JsonSerializer.Deserialize<List<BaseComponent>>(stateData, JsonOptions) ?? [];
    }
}