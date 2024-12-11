using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class ApiEndpointEntityTypeConfiguration : EFCoreEntityTypeConfiguration<ApiEndpoint>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<ApiEndpoint> builder)
    {
        builder
            .Property(p => p.Controller)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(p => p.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasMany(p => p.AppModules)
            .WithMany(p => p.ApiEndpoints)
            .UsingEntity<Dictionary<string, object>>("AppModuleEndpoints",
                j =>
                    j.HasOne<AppModule>()
                        .WithMany()
                        .HasForeignKey("AppModuleId")
                        .OnDelete(DeleteBehavior.Restrict),
                j =>
                    j.HasOne<ApiEndpoint>()
                        .WithMany()
                        .HasForeignKey("ApiEndpointId")
                        .OnDelete(DeleteBehavior.Restrict),
                j => j.ToTable("AppModuleEndpoints"));
        
        builder.ToTable("ApiEndpoints");
    }
}