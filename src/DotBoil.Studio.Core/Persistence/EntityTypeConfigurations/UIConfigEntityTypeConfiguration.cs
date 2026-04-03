using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using DotBoil.Studio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.Studio.Core.Persistence.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(StudioDbContext))]
public class UIConfigEntityTypeConfiguration : EFCoreEntityTypeConfiguration<UIConfig>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<UIConfig> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.StateData).IsRequired(false);
        builder.ToTable("UIConfigs");
    }
}