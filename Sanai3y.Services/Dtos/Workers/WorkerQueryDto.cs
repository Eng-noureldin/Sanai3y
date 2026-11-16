namespace Sanai3y.Services.Dtos.Workers;

public class WorkerQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public int? ProfessionId { get; set; }

    public decimal? MinRating { get; set; }
    public bool? IsAvailable { get; set; }
}