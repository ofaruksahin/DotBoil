using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class RoleApiEndpointEntityTypeConfiguration : EFCoreEntityTypeConfiguration<RoleApiEndpoint>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<RoleApiEndpoint> builder)
    {
        builder.ToTable("RoleApiEndpoints");
    }
}