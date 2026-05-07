using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.ControlCenter;

namespace Maba.Infrastructure.Data.Configurations;

public class AppRuntimeMetadataConfiguration : IEntityTypeConfiguration<AppRuntimeMetadata>
{
    public void Configure(EntityTypeBuilder<AppRuntimeMetadata> builder)
    {
        builder.ToTable("AppRuntimeMetadata");
        builder.ConfigureBaseEntity<AppRuntimeMetadata>();

        builder.Property(x => x.Channel).IsRequired().HasMaxLength(50).HasDefaultValue("stable");
        builder.Property(x => x.AppVersion).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FirmwareName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FirmwareVersion).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TargetBoard).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProtocolName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CommandSummary).HasMaxLength(2000);
        builder.Property(x => x.CompatibilityNotes).HasMaxLength(2000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
