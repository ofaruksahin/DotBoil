using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class UserEntityTypeConfiguration : EFCoreEntityTypeConfiguration<User>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<User> builder)
    {
        builder
            .Property(p => p.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(p => p.Surname)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(p => p.Username)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(p => p.Email)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(p => p.Password)
            .HasMaxLength(255)
            .IsRequired(false);

        builder
            .HasMany(p => p.Roles)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany<OtpCode>(p => p.OtpCodes)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.ToTable("Users");
    }
}