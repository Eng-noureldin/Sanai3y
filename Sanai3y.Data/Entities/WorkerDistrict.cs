namespace Sanai3y.Data.Entities;

public class WorkerDistrict
{
    public string WorkerId { get; set; } = null!;
    public int DistrictId { get; set; }

    // Navigation properties
    public WorkerProfile Worker { get; set; } = null!;
    public District District { get; set; } = null!;
}