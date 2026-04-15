using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Assets;

namespace Maba.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.ConfigureBaseEntity<Asset>();

        builder.Property(x => x.AssetNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.AssetNumber).IsUnique();
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.DescriptionEn).HasMaxLength(2000);
        builder.Property(x => x.DescriptionAr).HasMaxLength(2000);
        builder.Property(x => x.ConditionNotes).HasMaxLength(1000);
        builder.Property(x => x.LocationNotes).HasMaxLength(500);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("ILS");

        builder.HasOne(x => x.AssetCategory)
            .WithMany()
            .HasForeignKey(x => x.AssetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvestorUser)
            .WithMany()
            .HasForeignKey(x => x.InvestorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcquisitionCondition)
            .WithMany()
            .HasForeignKey(x => x.AcquisitionConditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PhotoMedia)
            .WithMany()
            .HasForeignKey(x => x.PhotoMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.ToTable("AssetCategories");
        builder.ConfigureBaseEntity<AssetCategory>();

        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NumberingPrefix).HasMaxLength(20);
    }
}

public class AssetNumberingSettingConfiguration : IEntityTypeConfiguration<AssetNumberingSetting>
{
    public void Configure(EntityTypeBuilder<AssetNumberingSetting> builder)
    {
        builder.ToTable("AssetNumberingSettings");
        builder.ConfigureBaseEntity<AssetNumberingSetting>();

        builder.Property(x => x.Prefix).IsRequired().HasMaxLength(20).HasDefaultValue("A-");
        builder.Property(x => x.PadWidth).HasDefaultValue(4);
        builder.Property(x => x.NextNumber).HasDefaultValue(1L);
    }
}
