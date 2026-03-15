using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Common;

namespace Maba.Infrastructure.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(s => s.DescriptionAr)
            .HasMaxLength(500);

        builder.Property(s => s.Category)
            .HasMaxLength(50);

        builder.Property(s => s.DataType)
            .HasMaxLength(20)
            .HasDefaultValue("String");

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.HasIndex(s => s.Category);
    }
}

