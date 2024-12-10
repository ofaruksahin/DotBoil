using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class ApiEndpoint : BaseEntity
{
    public string Controller { get; set; }
    public string Action { get; set; }

    public virtual ICollection<AppModule> AppModules { get; set; }
}