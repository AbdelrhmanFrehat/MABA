using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Sales;

namespace Maba.Infrastructure.Data.Configurations;

public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");
        builder.ConfigureBaseEntity<Quotation>();

        builder.Property(q => q.QuotationNumber).IsRequired().HasMaxLength(50);
        builder.Property(q => q.SourceRequestType).HasMaxLength(20);
        builder.Property(q => q.SourceRequestReference).HasMaxLength(50);
        builder.Property(q => q.Currency).HasMaxLength(3).HasDefaultValue("ILS");
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(q => q.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(q => q.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(q => q.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(q => q.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(q => q.Customer)
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ConvertedToOrderId is a denormalized reverse-lookup column, NOT the FK
        // for the one-to-one relationship (that FK is Order.SourceQuotationId,
        // configured in OrderConfiguration).
        // Tell EF Core to ignore the convention-based FK inference.
        builder.Property(q => q.ConvertedToOrderId);
        builder.HasIndex(q => q.ConvertedToOrderId)
            .HasFilter("[ConvertedToOrderId] IS NOT NULL");

        // The navigation Quotation.ConvertedToOrder is mapped by OrderConfiguration
        // via HasOne(o => o.SourceQuotation).WithOne(q => q.ConvertedToOrder)
        // — no additional config needed here.
    }
}

public class QuotationItemConfiguration : IEntityTypeConfiguration<QuotationItem>
{
    public void Configure(EntityTypeBuilder<QuotationItem> builder)
    {
        builder.ToTable("QuotationItems");
        builder.ConfigureBaseEntity<QuotationItem>();

        builder.Property(i => i.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountPercent).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxPercent).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Unit).HasMaxLength(20);
        builder.Property(i => i.Description).IsRequired();

        builder.HasOne(i => i.Quotation)
            .WithMany(q => q.Items)
            .HasForeignKey(i => i.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
