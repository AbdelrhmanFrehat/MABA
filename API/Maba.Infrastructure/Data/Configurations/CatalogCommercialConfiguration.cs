using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Catalog;

namespace Maba.Infrastructure.Data.Configurations;

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");
        builder.ConfigureBaseEntity<UnitOfMeasure>();

        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Abbreviation).IsRequired().HasMaxLength(20);
    }
}

public class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
    public void Configure(EntityTypeBuilder<UnitConversion> builder)
    {
        builder.ToTable("UnitConversions");
        builder.ConfigureBaseEntity<UnitConversion>();

        builder.HasOne(x => x.FromUnit)
            .WithMany(x => x.FromConversions)
            .HasForeignKey(x => x.FromUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToUnit)
            .WithMany(x => x.ToConversions)
            .HasForeignKey(x => x.ToUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ItemUnitConfiguration : IEntityTypeConfiguration<ItemUnit>
{
    public void Configure(EntityTypeBuilder<ItemUnit> builder)
    {
        builder.ToTable("ItemUnits");
        builder.ConfigureBaseEntity<ItemUnit>();

        builder.Property(x => x.Barcode).HasMaxLength(100);
        builder.HasIndex(x => new { x.ItemId, x.UnitOfMeasureId }).IsUnique();
    }
}

public class SupplierItemPriceConfiguration : IEntityTypeConfiguration<SupplierItemPrice>
{
    public void Configure(EntityTypeBuilder<SupplierItemPrice> builder)
    {
        builder.ToTable("SupplierItemPrices");
        builder.ConfigureBaseEntity<SupplierItemPrice>();

        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");
        builder.HasIndex(x => new { x.SupplierId, x.ItemId, x.UnitOfMeasureId, x.MinQuantity });

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.SupplierItemPrices)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
