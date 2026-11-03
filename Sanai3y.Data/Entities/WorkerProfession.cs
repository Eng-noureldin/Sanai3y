namespace Sanai3y.Data.Entities;

public class WorkerProfession
{
    public string WorkerId { get; set; } = null!;
    public int ProfessionId { get; set; }

    // Navigation properties
    public WorkerProfile Worker { get; set; } = null!;
    public Profession Profession { get; set; } = null!;
}