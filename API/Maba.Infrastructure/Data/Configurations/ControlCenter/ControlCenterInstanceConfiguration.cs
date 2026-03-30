using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class ControlCenterInstanceConfiguration : IEntityTypeConfiguration<ControlCenterInstance>
{
    public void Configure(EntityTypeBuilder<ControlCenterInstance> builder)
    {
        builder.ToTable("ControlCenterInstances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Hostname)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CoreVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OsInfo)
            .HasMaxLength(1000);

        builder.Property(x => x.InstalledModulesJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.OrgId, x.SiteId });
    }
}

