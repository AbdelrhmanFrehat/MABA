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

        builder.Property(x => x.JobReference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceReference)
            .HasMaxLength(100);

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(300);

        builder.Property(x => x.MachineType)
            .HasMaxLength(50);

        builder.Property(x => x.Priority)
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ResultSummary)
            .HasMaxLength(2000);

        builder.Property(x => x.ParametersJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AttachmentsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.JobReference)
            .IsUnique();

        builder.HasIndex(x => new { x.SourceType, x.SourceId });
        builder.HasIndex(x => new { x.MachineType, x.Status });
        builder.HasIndex(x => new { x.OrgId, x.SiteId, x.DeviceId });
    }
}

