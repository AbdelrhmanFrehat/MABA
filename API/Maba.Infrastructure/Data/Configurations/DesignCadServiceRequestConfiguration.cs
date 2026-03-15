using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.DesignCad;

namespace Maba.Infrastructure.Data.Configurations;

public class DesignCadServiceRequestConfiguration : IEntityTypeConfiguration<DesignCadServiceRequest>
{
    public void Configure(EntityTypeBuilder<DesignCadServiceRequest> builder)
    {
        builder.ToTable("DesignCadServiceRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.ReferenceNumber).IsUnique();

        builder.Property(r => r.Title).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Description).HasMaxLength(4000);
        builder.Property(r => r.IntendedUse).HasMaxLength(200);
        builder.Property(r => r.MaterialNotes).HasMaxLength(500);
        builder.Property(r => r.DimensionsNotes).HasMaxLength(1000);
        builder.Property(r => r.ToleranceNotes).HasMaxLength(1000);
        builder.Property(r => r.WhatNeedsChange).HasMaxLength(2000);
        builder.Property(r => r.CriticalSurfaces).HasMaxLength(1000);
        builder.Property(r => r.FitmentRequirements).HasMaxLength(1000);
        builder.Property(r => r.PurposeAndConstraints).HasMaxLength(2000);
        builder.Property(r => r.Deadline).HasMaxLength(100);
        builder.Property(r => r.CustomerNotes).HasMaxLength(2000);
        builder.Property(r => r.AdminNotes).HasMaxLength(4000);
        builder.Property(r => r.CustomerName).HasMaxLength(200);
        builder.Property(r => r.CustomerEmail).HasMaxLength(255);
        builder.Property(r => r.CustomerPhone).HasMaxLength(50);
        builder.Property(r => r.QuotedPrice).HasColumnType("decimal(18,2)");
        builder.Property(r => r.FinalPrice).HasColumnType("decimal(18,2)");

        builder.Property(r => r.RequestType).HasConversion<int>();
        builder.Property(r => r.TargetProcess).HasConversion<int>();
        builder.Property(r => r.Status).HasConversion<int>();

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
