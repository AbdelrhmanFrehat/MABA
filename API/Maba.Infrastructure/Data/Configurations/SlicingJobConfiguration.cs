using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Printing;

namespace Maba.Infrastructure.Data.Configurations;

public class SlicingJobConfiguration : IEntityTypeConfiguration<SlicingJob>
{
    public void Configure(EntityTypeBuilder<SlicingJob> builder)
    {
        builder.ToTable("SlicingJobs");
        builder.ConfigureBaseEntity<SlicingJob>();

        builder.Property(sj => sj.EstimatedMaterialGrams)
            .HasColumnType("decimal(18,2)");

        builder.Property(sj => sj.PriceEstimate)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(sj => sj.DesignFile)
            .WithMany(df => df.SlicingJobs)
            .HasForeignKey(sj => sj.DesignFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sj => sj.SlicingProfile)
            .WithMany(sp => sp.SlicingJobs)
            .HasForeignKey(sj => sj.SlicingProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sj => sj.SlicingJobStatus)
            .WithMany(s => s.SlicingJobs)
            .HasForeignKey(sj => sj.SlicingJobStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

