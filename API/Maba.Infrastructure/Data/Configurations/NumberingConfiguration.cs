using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Numbering;

namespace Maba.Infrastructure.Data.Configurations;

public class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences");
        builder.ConfigureBaseEntity<DocumentSequence>();

        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Prefix).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Separator).HasMaxLength(5);
        builder.HasIndex(x => x.DocumentType).IsUnique();
    }
}
