using DotBoil.Entities;

namespace DotBoil.Studio.Core.Entities;

public class UIConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StateData { get; set; }
}