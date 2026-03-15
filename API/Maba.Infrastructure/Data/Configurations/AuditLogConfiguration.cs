using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Common;

namespace Maba.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(al => al.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(al => al.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(al => al.UserEmail)
            .HasMaxLength(256);

        builder.Property(al => al.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.RequestPath)
            .HasMaxLength(500);

        builder.Property(al => al.RequestMethod)
            .HasMaxLength(10);

        builder.Property(al => al.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => new { al.EntityType, al.EntityId });

        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

