using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class OtpCodeEntityTypeConfiguration : EFCoreEntityTypeConfiguration<OtpCode>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<OtpCode> builder)
    {
        builder
            .Property(p => p.UserId)
            .IsRequired();

        builder
            .Property(p => p.Code)
            .HasMaxLength(6)
            .IsRequired();

        builder
            .Property(p => p.ExpiryDate)
            .IsRequired();

        builder
            .Property(p => p.IsUsed)
            .IsRequired();

        builder
            .Property(p => p.IsExpired)
            .IsRequired();

        builder
            .HasOne<User>(p => p.User)
            .WithMany(p => p.OtpCodes)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.ToTable("OtpCodes");
    }
}