using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        // One review per request
        builder.HasIndex(r => r.JobRequestId)
            .IsUnique();

        builder.Property(r => r.Comment)
            .HasMaxLength(500);

        builder.HasOne(r => r.JobRequest)
            .WithOne(j => j.Review)
            .HasForeignKey<Review>(r => r.JobRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Worker)
            .WithMany()
            .HasForeignKey(r => r.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}