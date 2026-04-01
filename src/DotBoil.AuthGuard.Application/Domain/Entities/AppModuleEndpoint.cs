using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class AppModuleEndpoint : BaseEntity
{
    public int AppModuleId { get; set; }
    public int ApiEndpointId { get; set; }

    public virtual AppModule AppModule { get; set; }
    public virtual ApiEndpoint ApiEndpoint { get; set; }
}