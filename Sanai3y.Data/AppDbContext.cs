using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sanai3y.Data.Entities;

namespace Sanai3y.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<WorkerProfession> WorkerProfessions => Set<WorkerProfession>();
    public DbSet<WorkerDistrict> WorkerDistricts => Set<WorkerDistrict>();
    public DbSet<JobRequest> JobRequests => Set<JobRequest>();
    public DbSet<RequestPhoto> RequestPhotos => Set<RequestPhoto>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}