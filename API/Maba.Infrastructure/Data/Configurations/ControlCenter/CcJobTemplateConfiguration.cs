using Maba.Domain.ControlCenter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations.ControlCenter;

public class CcJobTemplateConfiguration : IEntityTypeConfiguration<CcJobTemplate>
{
    public void Configure(EntityTypeBuilder<CcJobTemplate> builder)
    {
        builder.ToTable("CcJobTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.DeviceType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DefinitionJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasIndex(x => new { x.OrgId, x.SiteId });
    }
}

