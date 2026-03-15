using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.DesignCad;

namespace Maba.Infrastructure.Data.Configurations;

public class DesignCadServiceRequestAttachmentConfiguration : IEntityTypeConfiguration<DesignCadServiceRequestAttachment>
{
    public void Configure(EntityTypeBuilder<DesignCadServiceRequestAttachment> builder)
    {
        builder.ToTable("DesignCadServiceRequestAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => a.RequestId);
    }
}
