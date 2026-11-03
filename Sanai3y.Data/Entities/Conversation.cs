namespace Sanai3y.Data.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int JobRequestId { get; set; }
    public string CustomerId { get; set; } = null!;
    public string WorkerId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JobRequest JobRequest { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public WorkerProfile Worker { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public bool HasMember(string userId) => CustomerId == userId || WorkerId == userId;
}