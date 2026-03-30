using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class CcAuditEventConfiguration : IEntityTypeConfiguration<CcAuditEvent>
{
    public void Configure(EntityTypeBuilder<CcAuditEvent> builder)
    {
        builder.ToTable("CcAuditEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrgId, x.SiteId, x.Timestamp });
    }
}

