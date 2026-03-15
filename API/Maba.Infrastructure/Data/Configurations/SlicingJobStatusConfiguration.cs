using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Printing;

namespace Maba.Infrastructure.Data.Configurations;

public class SlicingJobStatusConfiguration : IEntityTypeConfiguration<SlicingJobStatus>
{
    public void Configure(EntityTypeBuilder<SlicingJobStatus> builder)
    {
        builder.ToTable("SlicingJobStatuses");
        builder.ConfigureBaseEntity<SlicingJobStatus>();

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.Property(s => s.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.NameAr)
            .IsRequired()
            .HasMaxLength(200);
    }
}

