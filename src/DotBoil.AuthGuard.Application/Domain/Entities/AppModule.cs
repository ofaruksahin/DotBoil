using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class AppModule : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<ApiEndpoint> ApiEndpoint { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
}