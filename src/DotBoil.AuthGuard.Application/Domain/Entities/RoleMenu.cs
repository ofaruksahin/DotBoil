using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class RoleMenu : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    
    public virtual Role Role { get; set; }
    public virtual Menu Menu { get; set; }
}