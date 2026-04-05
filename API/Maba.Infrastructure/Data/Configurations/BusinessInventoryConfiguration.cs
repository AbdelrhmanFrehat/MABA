using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Inventory;

namespace Maba.Infrastructure.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.ConfigureBaseEntity<Warehouse>();

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class InventoryCostLayerConfiguration : IEntityTypeConfiguration<InventoryCostLayer>
{
    public void Configure(EntityTypeBuilder<InventoryCostLayer> builder)
    {
        builder.ToTable("InventoryCostLayers");
        builder.ConfigureBaseEntity<InventoryCostLayer>();

        builder.Property(x => x.SourceDocumentType).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
