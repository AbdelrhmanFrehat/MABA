using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Laser;

namespace Maba.Infrastructure.Data.Configurations;

public class LaserServiceRequestConfiguration : IEntityTypeConfiguration<LaserServiceRequest>
{
    public void Configure(EntityTypeBuilder<LaserServiceRequest> builder)
    {
        builder.ToTable("LaserServiceRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.ReferenceNumber)
            .IsUnique();

        builder.Property(r => r.OperationMode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.ImageFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.CustomerName)
            .HasMaxLength(200);

        builder.Property(r => r.CustomerEmail)
            .HasMaxLength(255);

        builder.Property(r => r.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(r => r.CustomerNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.AdminNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.QuotedPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Status)
            .HasConversion<int>();

        builder.HasOne(r => r.Material)
            .WithMany()
            .HasForeignKey(r => r.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);
    }
}
