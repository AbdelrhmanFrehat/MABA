using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Printing;

namespace Maba.Infrastructure.Data.Configurations;

public class PrintJobStatusConfiguration : IEntityTypeConfiguration<PrintJobStatus>
{
    public void Configure(EntityTypeBuilder<PrintJobStatus> builder)
    {
        builder.ToTable("PrintJobStatuses");
        builder.ConfigureBaseEntity<PrintJobStatus>();

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

