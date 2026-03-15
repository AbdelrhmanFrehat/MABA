using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Printing;

namespace Maba.Infrastructure.Data.Configurations;

public class MaterialColorConfiguration : IEntityTypeConfiguration<MaterialColor>
{
    public void Configure(EntityTypeBuilder<MaterialColor> builder)
    {
        builder.ToTable("MaterialColors");
        builder.ConfigureBaseEntity<MaterialColor>();

        builder.Property(c => c.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.HexCode)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.SortOrder)
            .HasDefaultValue(0);

        builder.HasOne(c => c.Material)
            .WithMany(m => m.AvailableColors)
            .HasForeignKey(c => c.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.MaterialId, c.SortOrder });
    }
}
