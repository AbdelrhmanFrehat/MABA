using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Catalog;

namespace Maba.Infrastructure.Data.Configurations;

public class ItemStatusConfiguration : IEntityTypeConfiguration<ItemStatus>
{
    public void Configure(EntityTypeBuilder<ItemStatus> builder)
    {
        builder.ToTable("ItemStatuses");
        builder.ConfigureBaseEntity<ItemStatus>();

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

