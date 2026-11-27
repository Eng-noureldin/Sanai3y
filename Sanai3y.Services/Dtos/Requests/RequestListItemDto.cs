namespace Sanai3y.Services.Dtos.Requests;

public class RequestListItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ProfessionName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int QuotesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}