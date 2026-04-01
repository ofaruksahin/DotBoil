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
            .WithOne(p => p.ApiEndpoint)
            .HasForeignKey(p => p.ApiEndpointId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.ToTable("ApiEndpoints");
    }
}