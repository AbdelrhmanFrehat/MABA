using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class CcTelemetryRecordConfiguration : IEntityTypeConfiguration<CcTelemetryRecord>
{
    public void Configure(EntityTypeBuilder<CcTelemetryRecord> builder)
    {
        builder.ToTable("CcTelemetryRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MetricType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(50);

        builder.Property(x => x.TagsJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrgId, x.SiteId, x.Timestamp });
    }
}

