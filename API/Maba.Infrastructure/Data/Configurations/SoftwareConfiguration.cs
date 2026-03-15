using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Software;

namespace Maba.Infrastructure.Data.Configurations;

public class SoftwareProductConfiguration : IEntityTypeConfiguration<SoftwareProduct>
{
    public void Configure(EntityTypeBuilder<SoftwareProduct> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Slug).HasMaxLength(100).IsRequired();
        builder.Property(p => p.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(p => p.SummaryEn).HasMaxLength(500);
        builder.Property(p => p.SummaryAr).HasMaxLength(500);
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.IconKey).HasMaxLength(100);

        builder.HasMany(p => p.Releases)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SoftwareReleaseConfiguration : IEntityTypeConfiguration<SoftwareRelease>
{
    public void Configure(EntityTypeBuilder<SoftwareRelease> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Version).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(r => r.Files)
            .WithOne(f => f.Release)
            .HasForeignKey(f => f.ReleaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SoftwareFileConfiguration : IEntityTypeConfiguration<SoftwareFile>
{
    public void Configure(EntityTypeBuilder<SoftwareFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Os).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Arch).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.FileType).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.FileName).HasMaxLength(255).IsRequired();
        builder.Property(f => f.StoredPath).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Sha256).HasMaxLength(64);
    }
}
