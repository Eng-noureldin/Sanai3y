
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
        var workersQuery = _context.WorkerProfiles
            .AsNoTracking()
            .Where(w =>
                w.VerificationStatus == VerificationStatus.Approved &&
                !w.User.IsBlocked);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            workersQuery = workersQuery.Where(w =>
                w.User.FullName.Contains(query.Search));
        }

        if (query.CityId.HasValue)
        {
            workersQuery = workersQuery.Where(w =>
                w.CityId == query.CityId.Value);
        }

        if (query.DistrictId.HasValue)
        {
            workersQuery = workersQuery.Where(w =>
                w.Districts.Any(d =>
                    d.DistrictId == query.DistrictId.Value));
        }

        if (query.ProfessionId.HasValue)
        {
            workersQuery = workersQuery.Where(w =>
                w.Professions.Any(p =>
                    p.ProfessionId == query.ProfessionId.Value));
        }

        if (query.MinRating.HasValue)
        {
            workersQuery = workersQuery.Where(w =>
                w.AvgRating >= query.MinRating.Value);
        }

        if (query.IsAvailable.HasValue)
        {
            workersQuery = workersQuery.Where(w =>
                w.IsAvailable == query.IsAvailable.Value);
        }

        var totalCount = await workersQuery.CountAsync();

        var workers = await workersQuery
            .OrderByDescending(w => w.AvgRating)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(w => new WorkerListItemDto
            {
                Id = w.UserId,
                FullName = w.User.FullName,
                PhotoUrl = w.PhotoUrl,
                CityName = w.City.Name,
                AvgRating = w.AvgRating,
                RatingCount = w.RatingCount,
                IsAvailable = w.IsAvailable
            })
            .ToListAsync();

        return new PagedResult<WorkerListItemDto>
        {
            Items = workers,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<WorkerDetailsDto?> GetWorkerDetailsAsync(string id)
    {
        var worker = await _context.WorkerProfiles
            .AsNoTracking()
            .Where(w =>
                w.UserId == id &&
                w.VerificationStatus == VerificationStatus.Approved &&
                !w.User.IsBlocked)
            .Select(w => new WorkerDetailsDto
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

        if (worker == null)
            return null;

        worker.Reviews = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.WorkerId == id &&
                !r.IsHidden)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new WorkerReviewDto
            {
                CustomerName = r.Customer.FullName,
                Stars = r.Stars,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return worker;
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
        var worker = await _context.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Districts)
            .Include(w => w.Professions)
            .FirstOrDefaultAsync(w => w.UserId == workerId);

        if (worker == null)
            return false;

        worker.User.FullName = dto.FullName;
        worker.Bio = dto.Bio;
        worker.ExperienceYears = dto.ExperienceYears;
        worker.CityId = dto.CityId;
        worker.IsAvailable = dto.IsAvailable;

        if (dto.PhotoUrl != null)
            worker.PhotoUrl = dto.PhotoUrl;

        var cityExists = await _context.Cities
            .AnyAsync(c =>
                c.Id == dto.CityId &&
                c.IsActive);

        if (!cityExists)
            throw new BusinessException("Invalid city.");

        var districtsAreValid = await _context.Districts
            .Where(d => dto.DistrictIds.Contains(d.Id))
            .AllAsync(d =>
                d.IsActive &&
                d.CityId == dto.CityId);

        var districtCount = await _context.Districts
            .CountAsync(d => dto.DistrictIds.Contains(d.Id));

        if (!districtsAreValid ||
            districtCount != dto.DistrictIds.Distinct().Count())
        {
            throw new BusinessException("Invalid districts.");
        }

        var professionsAreValid = await _context.Professions
            .Where(p => dto.ProfessionIds.Contains(p.Id))
            .AllAsync(p => p.IsActive);

        var professionCount = await _context.Professions
            .CountAsync(p => dto.ProfessionIds.Contains(p.Id));

        if (!professionsAreValid ||
            professionCount != dto.ProfessionIds.Distinct().Count())
        {
            throw new BusinessException("Invalid professions.");
        }

        _context.WorkerDistricts.RemoveRange(worker.Districts);
        _context.WorkerProfessions.RemoveRange(worker.Professions);

        worker.Districts = dto.DistrictIds
            .Distinct()
            .Select(id => new WorkerDistrict
            {
                WorkerId = workerId,
                DistrictId = id
            })
            .ToList();

        worker.Professions = dto.ProfessionIds
            .Distinct()
            .Select(id => new WorkerProfession
            {
                WorkerId = workerId,
                ProfessionId = id
            })
            .ToList();

        await _context.SaveChangesAsync();

        return true;
    }
}
