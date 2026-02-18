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
            .HasMany(p => p.Menus)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>("RoleMenus",
                j =>
                    j.HasOne<Menu>()
                        .WithMany()
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Restrict),
                j =>
                    j.HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict),
                j => j.ToTable("RoleMenus"));

        builder
            .HasMany(p => p.AppModules)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>("RoleAppModules",
                j =>
                    j.HasOne<AppModule>()
                        .WithMany()
                        .HasForeignKey("AppModuleId")
                        .OnDelete(DeleteBehavior.Restrict),
                j =>
                    j.HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict),
                j => j.ToTable("RoleAppModules"));

        builder
            .HasMany(p => p.ApiEndpoints)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>("RoleApiEndpoints",
                j =>
                    j.HasOne<ApiEndpoint>()
                        .WithMany()
                        .HasForeignKey("ApiEndpointId")
                        .OnDelete(DeleteBehavior.Restrict),
                j =>
                    j.HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict),
                j => j.ToTable("RoleApiEndpoints"));

        builder.ToTable("Roles");
    }
}
