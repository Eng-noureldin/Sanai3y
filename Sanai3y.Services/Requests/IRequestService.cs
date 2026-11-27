using Sanai3y.Services.Dtos.Requests;

namespace Sanai3y.Services.Requests;

public interface IRequestService
{
    Task<int> CreateAsync(string customerId, CreateRequestDto dto);
    Task<List<RequestListItemDto>> GetMyRequestsAsync(string customerId, int page, int pageSize);
    Task<List<RequestListItemDto>> GetNearbyForWorkerAsync(string workerId, int page, int pageSize);
    Task<RequestDetailsDto> GetDetailsAsync(int id, string? userId);
    Task CancelAsync(int id, string userId, string? reason);
    Task CompleteAsync(int id, string customerId);
}