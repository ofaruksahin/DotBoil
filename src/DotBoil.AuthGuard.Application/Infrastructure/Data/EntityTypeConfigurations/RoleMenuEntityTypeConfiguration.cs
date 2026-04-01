using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class RoleMenuEntityTypeConfiguration : EFCoreEntityTypeConfiguration<RoleMenu>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("RoleMenus");
    }
}