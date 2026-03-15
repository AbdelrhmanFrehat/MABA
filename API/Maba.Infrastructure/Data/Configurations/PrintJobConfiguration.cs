using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Printing;

namespace Maba.Infrastructure.Data.Configurations;

public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.ToTable("PrintJobs");
        builder.ConfigureBaseEntity<PrintJob>();

        builder.Property(pj => pj.ActualMaterialGrams)
            .HasColumnType("decimal(18,2)");

        builder.Property(pj => pj.FinalPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(pj => pj.SlicingJob)
            .WithMany(sj => sj.PrintJobs)
            .HasForeignKey(pj => pj.SlicingJobId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

        builder.HasOne(pj => pj.Printer)
            .WithMany(p => p.PrintJobs)
            .HasForeignKey(pj => pj.PrinterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pj => pj.PrintJobStatus)
            .WithMany(s => s.PrintJobs)
            .HasForeignKey(pj => pj.PrintJobStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

