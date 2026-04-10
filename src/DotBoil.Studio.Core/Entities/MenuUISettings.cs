using DotBoil.Entities;
using DotBoil.Studio.Core.Enums;

namespace DotBoil.Studio.Core.Entities;

public class MenuUISettings : BaseEntity
{
    public int MenuId { get; set; }
    public MenuUISettingsType MenuType { get; set; } = MenuUISettingsType.StudioPage;
    public string Value { get; set; } = string.Empty;
}