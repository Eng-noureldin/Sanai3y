
using System.ComponentModel.DataAnnotations;

namespace Sanai3y.Services.Dtos.Workers;

public class UpdateWorkerProfileDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CityId { get; set; }

    [Required]
    public List<int> DistrictIds { get; set; } = new();

    [Required]
    public List<int> ProfessionIds { get; set; } = new();

    [Range(0, 80)]
    public int ExperienceYears { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsAvailable { get; set; }
}
