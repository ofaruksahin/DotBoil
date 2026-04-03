using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class MenuEntityTypeConfiguration : EFCoreEntityTypeConfiguration<Menu>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<Menu> builder)
    {
        builder
            .Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(p => p.Icon)
            .IsRequired(false);

        builder
            .Property(p => p.Path)
            .HasMaxLength(50)
            .IsRequired(false);

        builder
            .Property(p => p.Header)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(p => p.Rank)
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(p => p.ParentMenuId)
            .IsRequired(false);

        builder.ToTable("Menus");
    }
}