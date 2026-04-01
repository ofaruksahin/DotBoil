using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }

    public virtual ICollection<UserRole> Users { get; set; }
    public virtual ICollection<RoleMenu> Menus { get; set; }
    public virtual ICollection<RoleAppModule> AppModules { get; set; }
    public virtual ICollection<RoleApiEndpoint> ApiEndpoints { get; set; }
}
