using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class WorkerDistrictConfiguration : IEntityTypeConfiguration<WorkerDistrict>
{
    public void Configure(EntityTypeBuilder<WorkerDistrict> builder)
    {
        builder.HasKey(wd => new { wd.WorkerId, wd.DistrictId });

        builder.HasOne(wd => wd.Worker)
            .WithMany(w => w.Districts)
            .HasForeignKey(wd => wd.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wd => wd.District)
            .WithMany()
            .HasForeignKey(wd => wd.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}