using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(DotBoilAuthGuardDbContext))]
public class AppModuleEntityTypeConfiguration : EFCoreEntityTypeConfiguration<AppModule>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<AppModule> builder)
    {
        builder
            .Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.ToTable("AppModules");
    }
}