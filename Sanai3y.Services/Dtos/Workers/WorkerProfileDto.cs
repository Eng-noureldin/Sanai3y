
namespace Sanai3y.Services.Dtos.Workers;

public class WorkerProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Bio { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;
    public List<string> Districts { get; set; } = new();
    public List<string> Professions { get; set; } = new();

    public int ExperienceYears { get; set; }
    public decimal AvgRating { get; set; }
    public int RatingCount { get; set; }
    public int CompletedJobsCount { get; set; }
    public bool IsAvailable { get; set; }
}
