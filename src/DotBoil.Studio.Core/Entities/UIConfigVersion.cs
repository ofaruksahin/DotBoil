using DotBoil.Entities;

namespace DotBoil.Studio.Core.Entities;

public class UIConfigVersion : BaseEntity
{
    public int UIConfigId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StateData { get; set; }

    public UIConfig UIConfig { get; set; } = null!;
}