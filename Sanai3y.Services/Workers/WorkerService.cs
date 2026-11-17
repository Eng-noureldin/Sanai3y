
using Microsoft.EntityFrameworkCore;
using Sanai3y.Data;
using Sanai3y.Data.Enums;
using Sanai3y.Services.common;
using Sanai3y.Services.Dtos.Workers;

namespace Sanai3y.Services.Workers;

public class WorkerService : IWorkerService
{
    private readonly AppDbContext _context;

    public WorkerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WorkerListItemDto>> GetWorkersAsync(
        WorkerQueryDto query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var workers = _context.WorkerProfiles
            .AsNoTracking()
            .Where(w =>
                w.VerificationStatus == VerificationStatus.Approved &&
                !w.User.IsBlocked);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            workers = workers.Where(w =>
                w.User.FullName.Contains(query.Search));
        }

        if (query.CityId.HasValue)
        {
            workers = workers.Where(w =>
                w.CityId == query.CityId.Value);
        }

        if (query.DistrictId.HasValue)
        {
            workers = workers.Where(w =>
                w.Districts.Any(d =>
                    d.DistrictId == query.DistrictId.Value));
        }

        if (query.ProfessionId.HasValue)
        {
            workers = workers.Where(w =>
                w.Professions.Any(p =>
                    p.ProfessionId == query.ProfessionId.Value));
        }

        if (query.MinRating.HasValue)
        {
            workers = workers.Where(w =>
                w.AvgRating >= query.MinRating.Value);
        }

        if (query.IsAvailable.HasValue)
        {
            workers = workers.Where(w =>
                w.IsAvailable == query.IsAvailable.Value);
        }

        var totalCount = await workers.CountAsync();

        var items = await workers
            .OrderByDescending(w => w.AvgRating)
            .ThenBy(w => w.User.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkerListItemDto
            {
                Id = w.UserId,
                FullName = w.User.FullName,
                PhotoUrl = w.PhotoUrl,
                CityName = w.City.Name,
                Professions = w.Professions
                    .Select(p => p.Profession.Name)
                    .ToList(),
                ExperienceYears = w.ExperienceYears,
                AvgRating = w.AvgRating,
                RatingCount = w.RatingCount,
                CompletedJobsCount = w.CompletedJobsCount,
                IsAvailable = w.IsAvailable
            })
            .ToListAsync();

        return new PagedResult<WorkerListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<WorkerProfileDto?> GetProfileAsync(string workerId)
    {
        return await _context.WorkerProfiles
            .AsNoTracking()
            .Where(w =>
                w.UserId == workerId &&
                !w.User.IsBlocked)
            .Select(w => new WorkerProfileDto
            {
                Id = w.UserId,
                FullName = w.User.FullName,
                PhotoUrl = w.PhotoUrl,
                Bio = w.Bio,
                CityName = w.City.Name,

                Districts = w.Districts
                    .Select(d => d.District.Name)
                    .ToList(),

                Professions = w.Professions
                    .Select(p => p.Profession.Name)
                    .ToList(),

                ExperienceYears = w.ExperienceYears,
                AvgRating = w.AvgRating,
                RatingCount = w.RatingCount,
                CompletedJobsCount = w.CompletedJobsCount,
                IsAvailable = w.IsAvailable
            })
            .FirstOrDefaultAsync();
    }
}
