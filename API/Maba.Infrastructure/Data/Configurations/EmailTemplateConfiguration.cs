using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Common;

namespace Maba.Infrastructure.Data.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.SubjectEn)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.SubjectAr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.BodyHtmlEn)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.BodyHtmlAr)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.BodyTextEn)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.BodyTextAr)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Variables)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(t => t.Key)
            .IsUnique();

        builder.HasIndex(t => t.IsActive);
    }
}

