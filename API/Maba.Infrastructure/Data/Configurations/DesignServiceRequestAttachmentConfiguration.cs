using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Design;

namespace Maba.Infrastructure.Data.Configurations;

public class DesignServiceRequestAttachmentConfiguration : IEntityTypeConfiguration<DesignServiceRequestAttachment>
{
    public void Configure(EntityTypeBuilder<DesignServiceRequestAttachment> builder)
    {
        builder.ToTable("DesignServiceRequestAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => a.RequestId);
    }
}
