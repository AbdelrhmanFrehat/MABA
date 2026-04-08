using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.CommercialReturns;

namespace Maba.Infrastructure.Data.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("SalesReturns");
        builder.ConfigureBaseEntity<SalesReturn>();

        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(250);
        builder.Property(x => x.StatusKey).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StatusName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StatusColor).HasMaxLength(20);
        builder.Property(x => x.SalesInvoiceNumber).HasMaxLength(50);
        builder.Property(x => x.ReturnReasonLookupId).HasMaxLength(50);
        builder.Property(x => x.ReturnReasonName).HasMaxLength(100);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.HasIndex(x => x.ReturnNumber).IsUnique();

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.SalesReturn)
            .HasForeignKey(x => x.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
{
    public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
    {
        builder.ToTable("SalesReturnLines");
        builder.ConfigureBaseEntity<SalesReturnLine>();

        builder.Property(x => x.ItemName).HasMaxLength(250);
        builder.Property(x => x.ItemSku).HasMaxLength(100);
    }
}

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.ConfigureBaseEntity<PurchaseReturn>();

        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SupplierName).IsRequired().HasMaxLength(250);
        builder.Property(x => x.StatusKey).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StatusName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StatusColor).HasMaxLength(20);
        builder.Property(x => x.PurchaseInvoiceNumber).HasMaxLength(50);
        builder.Property(x => x.ReturnReasonLookupId).HasMaxLength(50);
        builder.Property(x => x.ReturnReasonName).HasMaxLength(100);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.HasIndex(x => x.ReturnNumber).IsUnique();

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.PurchaseReturn)
            .HasForeignKey(x => x.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("PurchaseReturnLines");
        builder.ConfigureBaseEntity<PurchaseReturnLine>();

        builder.Property(x => x.ItemName).HasMaxLength(250);
        builder.Property(x => x.ItemSku).HasMaxLength(100);
    }
}
