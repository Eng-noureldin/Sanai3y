using Sanai3y.Services.Dtos.Lookups;

namespace Sanai3y.Services.Lookups;

public interface ILookupService
{
    Task<IReadOnlyList<LookupItemDto>> GetCitiesAsync();

    Task<IReadOnlyList<LookupItemDto>> GetDistrictsAsync(int cityId);

    Task<IReadOnlyList<LookupItemDto>> GetProfessionsAsync();
}