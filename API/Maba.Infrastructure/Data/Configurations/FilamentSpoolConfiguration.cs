using Maba.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations;

public class FilamentSpoolConfiguration : IEntityTypeConfiguration<FilamentSpool>
{
    public void Configure(EntityTypeBuilder<FilamentSpool> builder)
    {
        builder.ToTable("FilamentSpools");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(200);

        builder.Property(e => e.InitialWeightGrams)
            .HasDefaultValue(1000);

        builder.Property(e => e.RemainingWeightGrams)
            .HasDefaultValue(1000);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MaterialColor)
            .WithMany()
            .HasForeignKey(e => e.MaterialColorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
