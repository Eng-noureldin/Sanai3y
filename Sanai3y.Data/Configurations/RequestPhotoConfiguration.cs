using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class RequestPhotoConfiguration : IEntityTypeConfiguration<RequestPhoto>
{
    public void Configure(EntityTypeBuilder<RequestPhoto> builder)
    {
        builder.Property(p => p.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(p => p.JobRequest)
            .WithMany(r => r.Photos)
            .HasForeignKey(p => p.JobRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}