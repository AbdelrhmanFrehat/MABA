using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Design;

namespace Maba.Infrastructure.Data.Configurations;

public class DesignServiceRequestConfiguration : IEntityTypeConfiguration<DesignServiceRequest>
{
    public void Configure(EntityTypeBuilder<DesignServiceRequest> builder)
    {
        builder.ToTable("DesignServiceRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.ReferenceNumber)
            .IsUnique();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Description)
            .HasMaxLength(4000);

        builder.Property(r => r.IntendedUse).HasMaxLength(100);
        builder.Property(r => r.MaterialPreference).HasMaxLength(500);
        builder.Property(r => r.DimensionsNotes).HasMaxLength(1000);
        builder.Property(r => r.BudgetRange).HasMaxLength(100);
        builder.Property(r => r.Timeline).HasMaxLength(100);

        builder.Property(r => r.CustomerName).HasMaxLength(200);
        builder.Property(r => r.CustomerEmail).HasMaxLength(255);
        builder.Property(r => r.CustomerPhone).HasMaxLength(50);

        builder.Property(r => r.AdminNotes).HasMaxLength(4000);
        builder.Property(r => r.QuotedPrice).HasColumnType("decimal(18,2)");
        builder.Property(r => r.FinalPrice).HasColumnType("decimal(18,2)");
        builder.Property(r => r.DeliveryNotes).HasMaxLength(2000);

        builder.Property(r => r.RequestType).HasConversion<int>();
        builder.Property(r => r.Status).HasConversion<int>();
        builder.Property(r => r.ToleranceLevel).HasConversion<int>();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Attachments)
            .WithOne(a => a.Request)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.RequestType);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
