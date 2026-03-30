using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class CcJobConfiguration : IEntityTypeConfiguration<CcJob>
{
    public void Configure(EntityTypeBuilder<CcJob> builder)
    {
        builder.ToTable("CcJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ResultSummary)
            .HasMaxLength(2000);

        builder.Property(x => x.ParametersJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrgId, x.SiteId, x.DeviceId });
    }
}

