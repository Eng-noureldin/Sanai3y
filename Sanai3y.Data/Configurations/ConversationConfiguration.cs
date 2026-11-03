using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.JobRequestId, c.CustomerId, c.WorkerId })
            .IsUnique();

        builder.HasOne(c => c.JobRequest)
            .WithMany()
            .HasForeignKey(c => c.JobRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Worker)
            .WithMany()
            .HasForeignKey(c => c.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}