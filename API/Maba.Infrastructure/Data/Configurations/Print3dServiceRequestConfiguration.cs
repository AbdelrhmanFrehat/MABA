using Maba.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maba.Infrastructure.Data.Configurations;

public class Print3dServiceRequestConfiguration : IEntityTypeConfiguration<Print3dServiceRequest>
{
    public void Configure(EntityTypeBuilder<Print3dServiceRequest> builder)
    {
        builder.HasOne(r => r.UsedSpool)
            .WithMany()
            .HasForeignKey(r => r.UsedSpoolId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
