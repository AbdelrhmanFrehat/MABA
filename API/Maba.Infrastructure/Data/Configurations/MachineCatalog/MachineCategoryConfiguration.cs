using Maba.Domain.MachineCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.MachineCatalog;

public class MachineCategoryConfiguration : IEntityTypeConfiguration<MachineCategory>
{
    public void Configure(EntityTypeBuilder<MachineCategory> builder)
    {
        builder.ToTable("MachineCatalogCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.DisplayNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DisplayNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(x => x.IconKey)
            .HasMaxLength(100);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(x => x.Families)
            .WithOne(f => f.Category)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
