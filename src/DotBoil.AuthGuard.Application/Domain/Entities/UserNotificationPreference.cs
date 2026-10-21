using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class UserNotificationPreference : BaseEntity
{
    public int UserId { get; set; }
    public string Channel { get; set; }
    public bool IsEnabled { get; set; }

    public virtual User User { get; set; }
}
