using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class CcDeviceConfiguration : IEntityTypeConfiguration<CcDevice>
{
    public void Configure(EntityTypeBuilder<CcDevice> builder)
    {
        builder.ToTable("CcDevices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SerialNumber)
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(500);

        builder.Property(x => x.TagsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrgId, x.SiteId });
        builder.HasIndex(x => x.SerialNumber);
    }
}

