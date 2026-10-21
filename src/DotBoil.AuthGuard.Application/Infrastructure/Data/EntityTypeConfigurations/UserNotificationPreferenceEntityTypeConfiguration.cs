using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class UserNotificationPreferenceEntityTypeConfiguration : EFCoreEntityTypeConfiguration<UserNotificationPreference>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder
            .Property(p => p.UserId)
            .IsRequired();

        builder
            .Property(p => p.Channel)
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(p => p.IsEnabled)
            .IsRequired();

        builder
            .HasIndex(p => new { p.UserId, p.Channel })
            .IsUnique();

        builder
            .HasOne<User>(p => p.User)
            .WithMany(p => p.NotificationPreferences)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("UserNotificationPreferences");
    }
}
