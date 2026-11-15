using Microsoft.EntityFrameworkCore;
using Sanai3y.Data;
using Sanai3y.Services.Dtos.Lookups;

namespace Sanai3y.Services.Lookups;

public class LookupService : ILookupService
{
    private readonly AppDbContext _context;

    public LookupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetCitiesAsync()
    {
        return await _context.Cities
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new LookupItemDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDistrictsAsync(int cityId)
    {
        return await _context.Districts
            .AsNoTracking()
            .Where(d => d.CityId == cityId && d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new LookupItemDto
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetProfessionsAsync()
    {
        return await _context.Professions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new LookupItemDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync();
    }
}