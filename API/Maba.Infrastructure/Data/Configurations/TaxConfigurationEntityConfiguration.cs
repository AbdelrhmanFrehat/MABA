using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Tax;

namespace Maba.Infrastructure.Data.Configurations;

public class TaxConfigurationEntityConfiguration : IEntityTypeConfiguration<TaxConfiguration>
{
    public void Configure(EntityTypeBuilder<TaxConfiguration> builder)
    {
        builder.ToTable("TaxConfigurations");
        builder.ConfigureBaseEntity<TaxConfiguration>();

        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
