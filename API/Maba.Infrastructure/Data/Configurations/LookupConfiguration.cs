using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maba.Domain.Lookups;

namespace Maba.Infrastructure.Data.Configurations;

public class LookupTypeConfiguration : IEntityTypeConfiguration<LookupType>
{
    public void Configure(EntityTypeBuilder<LookupType> builder)
    {
        builder.ToTable("LookupTypes");
        builder.ConfigureBaseEntity<LookupType>();

        builder.Property(x => x.Key).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Key).IsUnique();
    }
}

public class LookupValueConfiguration : IEntityTypeConfiguration<LookupValue>
{
    public void Configure(EntityTypeBuilder<LookupValue> builder)
    {
        builder.ToTable("LookupValues");
        builder.ConfigureBaseEntity<LookupValue>();

        builder.Property(x => x.Key).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Color).HasMaxLength(32);
        builder.Property(x => x.Icon).HasMaxLength(100);

        builder.HasIndex(x => new { x.LookupTypeId, x.Key }).IsUnique();

        builder.HasOne(x => x.LookupType)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.LookupTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
