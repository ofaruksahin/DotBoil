using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }

    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<Menu> Menus { get; set; }
    public virtual ICollection<AppModule> AppModules { get; set; }
}