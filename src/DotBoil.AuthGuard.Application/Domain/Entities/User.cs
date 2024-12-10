using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class User : BaseEntity
{
    public string Provider { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}