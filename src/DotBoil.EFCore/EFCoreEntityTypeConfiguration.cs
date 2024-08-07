using DotBoil.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotBoil.EFCore
{
    public abstract class EFCoreEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity
    {
        public abstract void ConfigureDotBoilEntity(EntityTypeBuilder<TEntity> builder);

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder
                .HasKey(p => p.Id);

            builder
                .Property(p => p.CreateUser)
                .IsRequired()
                .HasMaxLength(200);

            builder
                .Property(p => p.ModifyUser)
                .HasMaxLength(200);

            builder
                .Property(p => p.CreateTime)
                .IsRequired();

            builder
                .Property(p => p.UpdateTime);

            builder
                .Property(p => p.IsDeleted);

            builder
                .Ignore(p => p.Events);

            ConfigureDotBoilEntity(builder);
        }
    }
}
