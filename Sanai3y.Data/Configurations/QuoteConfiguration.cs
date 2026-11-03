using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(q => q.Note)
            .HasMaxLength(500)
            .IsRequired();

        // Unique constraint: One quote per worker per request
        builder.HasIndex(q => new { q.JobRequestId, q.WorkerId })
            .IsUnique();

        builder.HasOne(q => q.JobRequest)
            .WithMany(r => r.Quotes)
            .HasForeignKey(q => q.JobRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Worker)
            .WithMany()
            .HasForeignKey(q => q.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}