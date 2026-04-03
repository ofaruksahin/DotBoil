using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class Menu : BaseEntity
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Path { get; set; }
    public string Header { get; set; }
    public int Rank { get; set; }
    public int? ParentMenuId { get; set; }

    public virtual ICollection<RoleMenu> Roles { get; set; }
}