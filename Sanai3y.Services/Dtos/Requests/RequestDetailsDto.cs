namespace Sanai3y.Services.Dtos.Requests;

public class RequestDetailsDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ProfessionName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CancelReason { get; set; }
    public List<string> PhotoUrls { get; set; } = new();

    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? WorkerName { get; set; }
    public string? WorkerPhone { get; set; }
}