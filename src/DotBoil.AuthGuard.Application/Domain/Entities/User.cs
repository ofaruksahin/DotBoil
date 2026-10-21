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
    public DateTime? LastLoginDate { get; set; }
    public string? ActiveLanguage { get; set; }
    public bool IsPassive { get; set; }
    public DateTime? PassiveDate { get; set; }

    public virtual ICollection<UserRole> Roles { get; set; }
    public virtual ICollection<OtpCode> OtpCodes { get; set; }
    public virtual ICollection<UserNotificationPreference> NotificationPreferences { get; set; }
}