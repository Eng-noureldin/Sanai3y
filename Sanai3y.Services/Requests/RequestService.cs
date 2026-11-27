using Microsoft.EntityFrameworkCore;
using Sanai3y.Data;
using Sanai3y.Data.Entities;
using Sanai3y.Data.Enums;
using Sanai3y.Services.Common;
using Sanai3y.Services.Dtos.Requests;
using Sanai3y.Services.Files;

namespace Sanai3y.Services.Requests;

public class RequestService : IRequestService
{
    private readonly AppDbContext _db;
    private readonly IFileService _fileService;

    public RequestService(AppDbContext db, IFileService fileService)
    {
        _db = db;
        _fileService = fileService;
    }

    public async Task<int> CreateAsync(string customerId, CreateRequestDto dto)
    {
        if ((dto.Photos?.Count ?? 0) > 3)
            throw new BusinessException("You can upload up to 3 photos.");

        var professionExists = await _db.Professions
            .AnyAsync(p => p.Id == dto.ProfessionId && p.IsActive);
        if (!professionExists)
            throw new BusinessException("Invalid profession.");

        var districtExists = await _db.Districts
            .AnyAsync(d => d.Id == dto.DistrictId && d.IsActive);
        if (!districtExists)
            throw new BusinessException("Invalid district.");

        var now = DateTime.UtcNow;
        var request = new JobRequest
        {
            CustomerId = customerId,
            ProfessionId = dto.ProfessionId,
            DistrictId = dto.DistrictId,
            Description = dto.Description,
            Status = RequestStatus.Open,
            CreatedAt = now,
            ExpiresAt = now.AddDays(7)
        };

        if (dto.Photos != null)
        {
            foreach (var file in dto.Photos)
            {
                var url = await _fileService.SaveImageAsync(file, false);
                request.Photos.Add(new RequestPhoto { Url = url });
            }
        }

        _db.JobRequests.Add(request);
        await _db.SaveChangesAsync();

        var workerIds = await _db.WorkerProfiles
            .Where(w => w.VerificationStatus == VerificationStatus.Approved
                     && !w.User.IsBlocked
                     && w.Professions.Any(p => p.ProfessionId == dto.ProfessionId)
                     && w.Districts.Any(d => d.DistrictId == dto.DistrictId))
            .Select(w => w.UserId)
            .ToListAsync();

        var notifications = workerIds.Select(id => new Notification
        {
            UserId = id,
            Text = "New job request in your area",
            Link = $"/requests/{request.Id}"
        });
        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();

        return request.Id;
    }
    public async Task<List<RequestListItemDto>> GetMyRequestsAsync(string customerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        return await _db.JobRequests
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RequestListItemDto
            {
                Id = r.Id,
                Description = r.Description,
                ProfessionName = r.Profession.Name,
                DistrictName = r.District.Name,
                Status = r.Status.ToString(),
                QuotesCount = r.Quotes.Count,
                CreatedAt = r.CreatedAt,
                ExpiresAt = r.ExpiresAt
            })
            .ToListAsync();
    }

    public async Task<List<RequestListItemDto>> GetNearbyForWorkerAsync(string workerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var worker = await _db.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Professions)
            .Include(w => w.Districts)
            .FirstOrDefaultAsync(w => w.UserId == workerId);

        if (worker == null || !worker.CanWork())
            throw new BusinessException("Your account is not approved to view requests.");

        var professionIds = await _db.WorkerProfessions
            .Where(wp => wp.WorkerId == workerId && wp.Profession.IsActive)
            .Select(wp => wp.ProfessionId)
            .ToListAsync();

        var districtIds = await _db.WorkerDistricts
            .Where(wd => wd.WorkerId == workerId && wd.District.IsActive)
            .Select(wd => wd.DistrictId)
            .ToListAsync();
        var now = DateTime.UtcNow;

        return await _db.JobRequests
            .Where(r => r.Status == RequestStatus.Open
                     && r.ExpiresAt > now
                     && professionIds.Contains(r.ProfessionId)
                     && districtIds.Contains(r.DistrictId))
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RequestListItemDto
            {
                Id = r.Id,
                Description = r.Description,
                ProfessionName = r.Profession.Name,
                DistrictName = r.District.Name,
                Status = r.Status.ToString(),
                QuotesCount = r.Quotes.Count,
                CreatedAt = r.CreatedAt,
                ExpiresAt = r.ExpiresAt
            })
            .ToListAsync();
    }


    public async Task<RequestDetailsDto> GetDetailsAsync(int id, string? userId)
    {
        var request = await _db.JobRequests
            .Include(r => r.Customer)
            .Include(r => r.Profession)
            .Include(r => r.District)
            .Include(r => r.Photos)
            .Include(r => r.AcceptedQuote).ThenInclude(q => q!.Worker).ThenInclude(w => w.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            throw new BusinessException("Request not found.");

        var acceptedWorker = request.AcceptedQuote?.Worker.User;

        var dto = new RequestDetailsDto
        {
            Id = request.Id,
            Description = request.Description,
            ProfessionName = request.Profession.Name,
            DistrictName = request.District.Name,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt,
            ExpiresAt = request.ExpiresAt,
            CompletedAt = request.CompletedAt,
            CancelReason = request.CancelReason,
            PhotoUrls = request.Photos.Select(p => p.Url).ToList(),
            CustomerName = request.Customer.FullName,
            WorkerName = acceptedWorker?.FullName
        };

        var agreedOrCompleted = request.Status == RequestStatus.Agreed
                             || request.Status == RequestStatus.Completed;
        var isCustomer = userId != null && userId == request.CustomerId;
        var isAcceptedWorker = userId != null && acceptedWorker != null && userId == acceptedWorker.Id;

        if (agreedOrCompleted && (isCustomer || isAcceptedWorker))
        {
            dto.CustomerPhone = request.Customer.PhoneNumber;
            dto.WorkerPhone = acceptedWorker?.PhoneNumber;
        }

        return dto;
    }
    public async Task CancelAsync(int id, string userId, string? reason)
    {
        var request = await _db.JobRequests
            .Include(r => r.AcceptedQuote)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            throw new BusinessException("Request not found.");

        var isCustomer = request.CustomerId == userId;

        if (request.Status == RequestStatus.Open)
        {
            if (!isCustomer)
                throw new BusinessException("Only the request owner can cancel an open request.");
        }
        else if (request.Status == RequestStatus.Agreed)
        {
            var isAcceptedWorker = request.AcceptedQuote != null
                                && request.AcceptedQuote.WorkerId == userId;

            if (!isCustomer && !isAcceptedWorker)
                throw new BusinessException("You are not allowed to cancel this request.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessException("A cancellation reason is required.");
        }
        else
        {
            throw new BusinessException("This request can no longer be cancelled.");
        }

        request.Status = RequestStatus.Cancelled;
        request.CancelReason = reason;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("This request was already updated. Please refresh.");
        }
    }


    public async Task CompleteAsync(int id, string customerId)
    {
        var request = await _db.JobRequests
            .Include(r => r.AcceptedQuote).ThenInclude(q => q!.Worker)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            throw new BusinessException("Request not found.");

        if (request.CustomerId != customerId)
            throw new BusinessException("Only the request owner can complete it.");

        if (request.Status != RequestStatus.Agreed)
            throw new BusinessException("Only an agreed request can be completed.");

        request.Status = RequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;

        if (request.AcceptedQuote != null)
            request.AcceptedQuote.Worker.CompletedJobsCount++;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("This request was already updated. Please refresh.");
        }
    }
}