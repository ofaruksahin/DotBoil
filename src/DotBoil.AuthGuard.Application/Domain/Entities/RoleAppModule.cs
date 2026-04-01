using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class RoleAppModule : BaseEntity
{
    public int AppModuleId { get; set; }
    public int RoleId { get; set; }

    public virtual AppModule AppModule { get; set; }
    public virtual Role Role { get; set; }
}