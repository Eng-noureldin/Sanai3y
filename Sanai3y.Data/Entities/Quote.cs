using Sanai3y.Data.Enums;

namespace Sanai3y.Data.Entities;

public class Quote
{
    public int Id { get; set; }
    public int JobRequestId { get; set; }
    public string WorkerId { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string Note { get; set; } = null!; // MaxLength 500
    public QuoteStatus Status { get; set; } = QuoteStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JobRequest JobRequest { get; set; } = null!;
    public WorkerProfile Worker { get; set; } = null!;

    // Helper method mentioned in rules
    public bool IsPending() => Status == QuoteStatus.Pending;
}