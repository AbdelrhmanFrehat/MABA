using Maba.Domain.MachineCatalog;
using Maba.Domain.MachineCatalog.Sections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maba.Infrastructure.Data.Configurations.MachineCatalog;

public class MachineDefinitionConfiguration : IEntityTypeConfiguration<MachineDefinition>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public void Configure(EntityTypeBuilder<MachineDefinition> builder)
    {
        builder.ToTable("MachineCatalogDefinitions");
        builder.HasKey(x => x.Id);

        // Identity columns
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.RevisionNote)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayNameEn)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.DisplayNameAr)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.DescriptionEn)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.DescriptionAr)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Manufacturer)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("MABA");

        builder.Property(x => x.TagsJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("[]");

        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsPublic).HasDefaultValue(true);
        builder.Property(x => x.IsDeprecated).HasDefaultValue(false);

        builder.Property(x => x.DeprecationNote)
            .HasMaxLength(500);

        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.Property(x => x.InternalNotes)
            .HasColumnType("nvarchar(max)");

        // Foreign keys
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Family)
            .WithMany(f => f.Definitions)
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => new { x.IsActive, x.IsPublic, x.IsDeprecated });

        // Section JSON columns via value converters
        builder.Property(x => x.RuntimeBinding)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<RuntimeBindingSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AxisConfig)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<AxisConfigSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Workspace)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<WorkspaceSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.MotionDefaults)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<MotionDefaultsSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ConnectionDefaults)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<ConnectionDefaultsSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Capabilities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<CapabilitiesSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.FileSupport)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<FileSupportSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Visualization)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<VisualizationSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ProfileRules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<ProfileRulesSection>(v, JsonOptions) ?? new())
            .HasColumnType("nvarchar(max)");
    }
}
