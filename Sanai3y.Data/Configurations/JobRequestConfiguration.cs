using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class JobRequestConfiguration : IEntityTypeConfiguration<JobRequest>
{
    public void Configure(EntityTypeBuilder<JobRequest> builder)
    {
        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.CancelReason)
            .HasMaxLength(500);

        // Optimistic Concurrency check using RowVersion
        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // Foreign keys and Delete behaviors
        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Profession)
            .WithMany()
            .HasForeignKey(r => r.ProfessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.District)
            .WithMany()
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        // Required rule from Section 7: JobRequest.AcceptedQuoteId is NoAction
        builder.HasOne(r => r.AcceptedQuote)
            .WithMany()
            .HasForeignKey(r => r.AcceptedQuoteId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}