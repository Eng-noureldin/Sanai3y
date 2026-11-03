using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(u => u.WorkerProfile)
            .WithOne(w => w.User)
            .HasForeignKey<WorkerProfile>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}