using DotBoil.EFCore;
using DotBoil.EFCore.Attributes;
using DotBoil.Studio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.Studio.Core.Persistence.EntityTypeConfigurations;

[DotBoilEntityTypeConfiguration(typeof(StudioDbContext))]
public class UIConfigVersionEntityTypeConfiguration : EFCoreEntityTypeConfiguration<UIConfigVersion>
{
    public override void ConfigureDotBoilEntity(EntityTypeBuilder<UIConfigVersion> builder)
    {
        builder.Property(p => p.UIConfigId).IsRequired();
        builder.Property(p => p.Version).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.StateData).IsRequired(false);

        builder.HasOne(p => p.UIConfig)
               .WithMany()
               .HasForeignKey(p => p.UIConfigId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("UIConfigVersions");
    }
}