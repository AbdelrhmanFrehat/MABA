using Maba.Domain.MachineCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.MachineCatalog;

public class MachineFamilyConfiguration : IEntityTypeConfiguration<MachineFamily>
{
    public void Configure(EntityTypeBuilder<MachineFamily> builder)
    {
        builder.ToTable("MachineCatalogFamilies");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.DisplayNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DisplayNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(x => x.Manufacturer)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("MABA");

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Families)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Definitions)
            .WithOne(d => d.Family)
            .HasForeignKey(d => d.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId);
    }
}
