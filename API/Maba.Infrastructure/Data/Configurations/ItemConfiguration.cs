using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Catalog;

namespace Maba.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.NameEn)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.NameAr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(i => i.Sku)
            .IsUnique();

        builder.Property(i => i.GeneralDescriptionEn)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.GeneralDescriptionAr)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("ILS");

        builder.Property(i => i.AverageRating)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0);

        builder.Property(i => i.ReviewsCount)
            .HasDefaultValue(0);

        builder.Property(i => i.ViewsCount)
            .HasDefaultValue(0);

        builder.HasOne(i => i.ItemStatus)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.ItemStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Brand)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.ItemTags)
            .WithOne(it => it.Item)
            .HasForeignKey(it => it.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ItemSections)
            .WithOne(s => s.Item)
            .HasForeignKey(s => s.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Inventory)
            .WithOne(inv => inv.Item)
            .HasForeignKey<Inventory>(inv => inv.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

