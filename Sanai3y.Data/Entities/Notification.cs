namespace Sanai3y.Data.Entities;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Text { get; set; } = null!; // MaxLength 300
    public string Link { get; set; } = null!; // MaxLength 200
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // التصحيح DateTime.UtcNow

    // Navigation properties
    public User User { get; set; } = null!;
}