using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class RoleApiEndpoint : BaseEntity
{
    public int RoleId { get; set; }
    public int ApiEndpointId { get; set; }

    public virtual Role Role { get; set; }
    public virtual ApiEndpoint ApiEndpoint { get; set; }
}