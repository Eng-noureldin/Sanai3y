
using Sanai3y.Services.common;
using Sanai3y.Services.Dtos.Workers;

namespace Sanai3y.Services.Workers;

public interface IWorkerService
{
    Task<PagedResult<WorkerListItemDto>> GetWorkersAsync(WorkerQueryDto query);

    Task<WorkerProfileDto?> GetProfileAsync(string workerId);

    Task<bool> UpdateProfileAsync(
        string workerId,
        UpdateWorkerProfileDto dto);
}
