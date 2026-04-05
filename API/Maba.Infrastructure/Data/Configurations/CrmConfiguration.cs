using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Crm;

namespace Maba.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.ConfigureBaseEntity<Customer>();

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(250);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Phone2).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.CustomerType)
            .WithMany()
            .HasForeignKey(x => x.CustomerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.PriceList)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.ConfigureBaseEntity<Supplier>();

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(250);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Phone2).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.SupplierType)
            .WithMany()
            .HasForeignKey(x => x.SupplierTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
