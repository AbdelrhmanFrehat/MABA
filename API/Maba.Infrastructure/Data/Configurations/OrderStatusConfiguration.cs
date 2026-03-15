using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Orders;

namespace Maba.Infrastructure.Data.Configurations;

public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ToTable("OrderStatuses");
        builder.ConfigureBaseEntity<OrderStatus>();

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.Property(s => s.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.NameAr)
            .IsRequired()
            .HasMaxLength(200);
    }
}

