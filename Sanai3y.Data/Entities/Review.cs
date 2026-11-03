namespace Sanai3y.Data.Entities;

public class Review
{
    public int Id { get; set; }
    public int JobRequestId { get; set; }
    public string CustomerId { get; set; } = null!;
    public string WorkerId { get; set; } = null!;
    public int Stars { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsHidden { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JobRequest JobRequest { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public WorkerProfile Worker { get; set; } = null!;
}