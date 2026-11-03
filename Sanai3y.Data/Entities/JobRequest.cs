using Sanai3y.Data.Enums;

namespace Sanai3y.Data.Entities;

public class JobRequest
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int ProfessionId { get; set; }
    public int DistrictId { get; set; }
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Open;
    public int? AcceptedQuoteId { get; set; }
    public string? CancelReason { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = null!;

    // Navigation properties
    public User Customer { get; set; } = null!;
    public Profession Profession { get; set; } = null!;
    public District District { get; set; } = null!;
    public Quote? AcceptedQuote { get; set; }
    public ICollection<RequestPhoto> Photos { get; set; } = new List<RequestPhoto>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public Review? Review { get; set; }

    // Helper method from context file (Section 7)
    public bool IsOpen(DateTime now) => Status == RequestStatus.Open && now < ExpiresAt;
}