using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sanai3y.Data.Entities;
using Sanai3y.Data.Enums;

namespace Sanai3y.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Roles Seeding
        string[] roles = [nameof(UserRole.Admin), nameof(UserRole.Customer), nameof(UserRole.Worker)];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Lookups (City, Districts, Professions)
        if (!await context.Cities.AnyAsync())
        {
            var newCairo = new City
            {
                Name = "New Cairo",
                IsActive = true,
                Districts = new List<District>
                {
                    new() { Name = "First Settlement", IsActive = true },
                    new() { Name = "Fifth Settlement", IsActive = true },
                    new() { Name = "El Rehab", IsActive = true },
                    new() { Name = "Madinaty", IsActive = true }
                }
            };

            await context.Cities.AddAsync(newCairo);

            var professions = new List<Profession>
            {
                new() { Name = "Plumbing", IsActive = true },
                new() { Name = "Electrical", IsActive = true },
                new() { Name = "Carpentry", IsActive = true },
                new() { Name = "AC repair", IsActive = true },
                new() { Name = "Painting", IsActive = true }
            };

            await context.Professions.AddRangeAsync(professions);
            await context.SaveChangesAsync();
        }

        var city = await context.Cities.Include(c => c.Districts).FirstAsync(c => c.Name == "New Cairo");
        var districts = city.Districts.ToList();
        var plumbing = await context.Professions.FirstAsync(p => p.Name == "Plumbing");
        var electrical = await context.Professions.FirstAsync(p => p.Name == "Electrical");
        var carpentry = await context.Professions.FirstAsync(p => p.Name == "Carpentry");
        var ac = await context.Professions.FirstAsync(p => p.Name == "AC repair");
        var painting = await context.Professions.FirstAsync(p => p.Name == "Painting");

        const string defaultPassword = "Demo@1234";

        // 3. Admin Account
        if (await userManager.FindByEmailAsync("admin@sanai3y.test") == null)
        {
            var admin = new User
            {
                UserName = "admin@sanai3y.test",
                Email = "admin@sanai3y.test",
                FullName = "System Admin",
                Role = UserRole.Admin,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, defaultPassword);
            await userManager.AddToRoleAsync(admin, nameof(UserRole.Admin));
        }

        // 4. Customers
        User? mona = await userManager.FindByEmailAsync("mona@sanai3y.test");
        if (mona == null)
        {
            mona = new User
            {
                UserName = "mona@sanai3y.test",
                Email = "mona@sanai3y.test",
                FullName = "Mona Ahmed",
                PhoneNumber = "01011112222",
                Role = UserRole.Customer,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(mona, defaultPassword);
            await userManager.AddToRoleAsync(mona, nameof(UserRole.Customer));
        }

        if (await userManager.FindByEmailAsync("karim@sanai3y.test") == null)
        {
            var karim = new User
            {
                UserName = "karim@sanai3y.test",
                Email = "karim@sanai3y.test",
                FullName = "Karim Taha",
                PhoneNumber = "01033334444",
                Role = UserRole.Customer,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(karim, defaultPassword);
            await userManager.AddToRoleAsync(karim, nameof(UserRole.Customer));
        }

        // Helper method to seed workers
        async Task<User> SeedWorkerAsync(string email, string fullName, string phone, VerificationStatus status, Profession profession, string? rejectReason = null)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

            var user = new User
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phone,
                Role = UserRole.Worker,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(user, defaultPassword);
            await userManager.AddToRoleAsync(user, nameof(UserRole.Worker));

            var profile = new WorkerProfile
            {
                UserId = user.Id,
                CityId = city.Id,
                Bio = $"Professional {profession.Name} technician in New Cairo",
                ExperienceYears = 5,
                IsAvailable = true,
                VerificationStatus = status,
                RejectReason = rejectReason,
                CompletedJobsCount = status == VerificationStatus.Approved ? 12 : 0,
                AvgRating = status == VerificationStatus.Approved ? 4.8m : 0.0m,
                RatingCount = status == VerificationStatus.Approved ? 10 : 0
            };

            profile.Professions.Add(new WorkerProfession { ProfessionId = profession.Id, WorkerId = user.Id });
            foreach (var d in districts)
            {
                profile.Districts.Add(new WorkerDistrict { DistrictId = d.Id, WorkerId = user.Id });
            }

            await context.WorkerProfiles.AddAsync(profile);
            await context.SaveChangesAsync();
            return user;
        }

        // 5. Approved Workers
        var ahmedPlumber = await SeedWorkerAsync("ahmed.plumber@sanai3y.test", "Ahmed Plumber", "01112345678", VerificationStatus.Approved, plumbing);
        await SeedWorkerAsync("sayed.electric@sanai3y.test", "Sayed Electrician", "01122345678", VerificationStatus.Approved, electrical);
        await SeedWorkerAsync("hany.carpenter@sanai3y.test", "Hany Carpenter", "01132345678", VerificationStatus.Approved, carpentry);
        await SeedWorkerAsync("omar.ac@sanai3y.test", "Omar AC", "01142345678", VerificationStatus.Approved, ac);

        // 6. Pending and Rejected Workers
        await SeedWorkerAsync("tamer.painter@sanai3y.test", "Tamer Painter", "01152345678", VerificationStatus.Pending, painting);
        await SeedWorkerAsync("walid.plumber@sanai3y.test", "Walid Plumber", "01162345678", VerificationStatus.Rejected, plumbing, "National ID image is blurry.");

        // 7. Mona's Requests (one in each state: Open, Agreed, Completed, Cancelled, Expired)
        if (!await context.JobRequests.AnyAsync(r => r.CustomerId == mona.Id))
        {
            var firstDistrict = districts[0];

            // 1. Open Request
            var openRequest = new JobRequest
            {
                CustomerId = mona.Id,
                ProfessionId = plumbing.Id,
                DistrictId = firstDistrict.Id,
                Description = "Fixing kitchen sink pipe leak and tap installation.",
                Status = RequestStatus.Open,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            await context.JobRequests.AddAsync(openRequest);

            // 2. Agreed Request (with an accepted quote)
            var agreedRequest = new JobRequest
            {
                CustomerId = mona.Id,
                ProfessionId = plumbing.Id,
                DistrictId = firstDistrict.Id,
                Description = "Bathroom water mixer replacement.",
                Status = RequestStatus.Agreed,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5)
            };
            await context.JobRequests.AddAsync(agreedRequest);
            await context.SaveChangesAsync();

            var quoteAgreed = new Quote
            {
                JobRequestId = agreedRequest.Id,
                WorkerId = ahmedPlumber.Id,
                Price = 350.00m,
                DurationDays = 1,
                Note = "I can bring original Italian parts if needed.",
                Status = QuoteStatus.Accepted,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            await context.Quotes.AddAsync(quoteAgreed);
            await context.SaveChangesAsync();

            agreedRequest.AcceptedQuoteId = quoteAgreed.Id;

            // 3. Completed Request
            var completedRequest = new JobRequest
            {
                CustomerId = mona.Id,
                ProfessionId = plumbing.Id,
                DistrictId = firstDistrict.Id,
                Description = "Installed new water heater and pipes.",
                Status = RequestStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-3),
                CompletedAt = DateTime.UtcNow.AddDays(-2)
            };
            await context.JobRequests.AddAsync(completedRequest);
            await context.SaveChangesAsync();

            var quoteCompleted = new Quote
            {
                JobRequestId = completedRequest.Id,
                WorkerId = ahmedPlumber.Id,
                Price = 500.00m,
                DurationDays = 2,
                Note = "Complete installation with warranty.",
                Status = QuoteStatus.Accepted,
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            };
            await context.Quotes.AddAsync(quoteCompleted);
            await context.SaveChangesAsync();

            completedRequest.AcceptedQuoteId = quoteCompleted.Id;

            // Review for the completed request
            await context.Reviews.AddAsync(new Review
            {
                JobRequestId = completedRequest.Id,
                CustomerId = mona.Id,
                WorkerId = ahmedPlumber.Id,
                Stars = 5,
                Comment = "Excellent and fast work!",
                IsHidden = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            // 4. Cancelled Request
            var cancelledRequest = new JobRequest
            {
                CustomerId = mona.Id,
                ProfessionId = plumbing.Id,
                DistrictId = firstDistrict.Id,
                Description = "Repairing garden sprinkler network.",
                Status = RequestStatus.Cancelled,
                CancelReason = "Postponed travel reasons.",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                ExpiresAt = DateTime.UtcNow.AddDays(3)
            };
            await context.JobRequests.AddAsync(cancelledRequest);

            // 5. Expired Request
            var expiredRequest = new JobRequest
            {
                CustomerId = mona.Id,
                ProfessionId = plumbing.Id,
                DistrictId = firstDistrict.Id,
                Description = "Water tank maintenance.",
                Status = RequestStatus.Expired,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                ExpiresAt = DateTime.UtcNow.AddDays(-7)
            };
            await context.JobRequests.AddAsync(expiredRequest);

            await context.SaveChangesAsync();
        }
    }
}