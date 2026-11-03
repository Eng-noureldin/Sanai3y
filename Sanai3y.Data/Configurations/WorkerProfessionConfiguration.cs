using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class WorkerProfessionConfiguration : IEntityTypeConfiguration<WorkerProfession>
{
    public void Configure(EntityTypeBuilder<WorkerProfession> builder)
    {
        builder.HasKey(wp => new { wp.WorkerId, wp.ProfessionId });

        builder.HasOne(wp => wp.Worker)
            .WithMany(w => w.Professions)
            .HasForeignKey(wp => wp.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wp => wp.Profession)
            .WithMany()
            .HasForeignKey(wp => wp.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}