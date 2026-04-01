using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class RoleEntityTypeConfiguration : EFCoreEntityTypeConfiguration<Role>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<Role> builder)
    {
        builder
            .Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(p => p.IsDefault)
            .IsRequired();
        
        

        builder
            .HasMany(r => r.Menus)
            .WithOne(m => m.Role)
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(r => r.AppModules)
            .WithOne(m => m.Role)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(r => r.ApiEndpoints)
            .WithOne(a => a.Role)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("Roles");
    }
}
