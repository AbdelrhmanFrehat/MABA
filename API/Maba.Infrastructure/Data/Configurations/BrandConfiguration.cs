using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Catalog;

namespace Maba.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.ConfigureBaseEntity<Brand>();

        builder.Property(b => b.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);
    }
}

