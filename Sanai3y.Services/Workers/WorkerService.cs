
using Microsoft.EntityFrameworkCore;
using Sanai3y.Data;
using Sanai3y.Data.Entities;
using Sanai3y.Data.Enums;
using Sanai3y.Services.common;
using Sanai3y.Services.Common;
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

    public async Task<bool> UpdateProfileAsync(
        string workerId,
        UpdateWorkerProfileDto dto)
    {
        // Find worker and load related data
        var worker = await _context.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Districts)
            .Include(w => w.Professions)
            .FirstOrDefaultAsync(w => w.UserId == workerId);

        if (worker is null || worker.User.IsBlocked)
            throw new BusinessException("Worker profile not found.");

        // Validate full name
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new BusinessException("Full name is required.");

        // Validate selected city
        var cityExists = await _context.Cities
            .AnyAsync(c => c.Id == dto.CityId && c.IsActive);

        if (!cityExists)
            throw new BusinessException(
                "Selected city does not exist or is inactive.");

        // Validate selected districts
        var districtIds = dto.DistrictIds.Distinct().ToList();

        var validDistrictCount = await _context.Districts
            .CountAsync(d =>
                districtIds.Contains(d.Id) &&
                d.CityId == dto.CityId &&
                d.IsActive);

        if (validDistrictCount != districtIds.Count)
            throw new BusinessException(
                "One or more selected districts are invalid or inactive.");

        // Validate selected professions
        var professionIds = dto.ProfessionIds.Distinct().ToList();

        var validProfessionCount = await _context.Professions
            .CountAsync(p =>
                professionIds.Contains(p.Id) &&
                p.IsActive);

        if (validProfessionCount != professionIds.Count)
            throw new BusinessException(
                "One or more selected professions are invalid or inactive.");

        // Update user information
        worker.User.FullName = dto.FullName.Trim();

        // Update worker profile information
        worker.CityId = dto.CityId;
        worker.Bio = dto.Bio;
        worker.ExperienceYears = dto.ExperienceYears;
        worker.PhotoUrl = dto.PhotoUrl;
        worker.IsAvailable = dto.IsAvailable;

        // Synchronize districts
        _context.WorkerDistricts.RemoveRange(worker.Districts);

        worker.Districts = districtIds
            .Select(districtId => new WorkerDistrict
            {
                WorkerId = workerId,
                DistrictId = districtId
            })
            .ToList();

        // Synchronize professions
        _context.WorkerProfessions.RemoveRange(worker.Professions);

        worker.Professions = professionIds
            .Select(professionId => new WorkerProfession
            {
                WorkerId = workerId,
                ProfessionId = professionId
            })
            .ToList();

        // Save changes
        await _context.SaveChangesAsync();

        return true;
    }
}
