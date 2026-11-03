using Sanai3y.Data.Enums;

namespace Sanai3y.Data.Entities;

public class WorkerProfile
{
    public string UserId { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsAvailable { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;
    public string? IdPhotoPath { get; set; }
    public string? SelfiePath { get; set; }
    public string? RejectReason { get; set; }
    public int CompletedJobsCount { get; set; }
    public decimal AvgRating { get; set; }
    public int RatingCount { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public City City { get; set; } = null!;
    public ICollection<WorkerProfession> Professions { get; set; } = new List<WorkerProfession>();
    public ICollection<WorkerDistrict> Districts { get; set; } = new List<WorkerDistrict>();

    // Helper method from context file (Section 7)
    public bool CanWork() => VerificationStatus == VerificationStatus.Approved && !User.IsBlocked;
}