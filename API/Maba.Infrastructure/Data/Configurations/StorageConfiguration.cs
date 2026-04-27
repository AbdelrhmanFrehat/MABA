using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Storage;

namespace Maba.Infrastructure.Data.Configurations;

public class StorageParentConfiguration : IEntityTypeConfiguration<StorageParent>
{
    public void Configure(EntityTypeBuilder<StorageParent> builder)
    {
        builder.ToTable("StorageParents");
        builder.ConfigureBaseEntity<StorageParent>();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(100).HasDefaultValue("Other");
        builder.Property(x => x.Manufacturer).HasMaxLength(200);
        builder.Property(x => x.ImageUrl).HasMaxLength(2000);
        builder.Property(x => x.DatasheetUrl).HasMaxLength(2000);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsPublishedToShop).HasDefaultValue(false);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StorageVariantConfiguration : IEntityTypeConfiguration<StorageVariant>
{
    public void Configure(EntityTypeBuilder<StorageVariant> builder)
    {
        builder.ToTable("StorageVariants");
        builder.ConfigureBaseEntity<StorageVariant>();

        builder.Property(x => x.VariantLabel).HasMaxLength(500);
        builder.Property(x => x.Sku).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(50).HasDefaultValue("pcs");
        builder.Property(x => x.Package).HasMaxLength(100);
        builder.Property(x => x.Value).HasMaxLength(100);
        builder.Property(x => x.ValueUnit).HasMaxLength(50);
        builder.Property(x => x.Tolerance).HasMaxLength(50);
        builder.Property(x => x.VoltageRating).HasMaxLength(50);
        builder.Property(x => x.CurrentRating).HasMaxLength(50);
        builder.Property(x => x.PowerRating).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(2000);
        builder.Property(x => x.DatasheetUrl).HasMaxLength(2000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsPublishedToShop).HasDefaultValue(false);

        builder.HasIndex(x => x.Sku).IsUnique();
    }
}
