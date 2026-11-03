using Microsoft.AspNetCore.Identity;
using Sanai3y.Data.Enums;

namespace Sanai3y.Data.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public WorkerProfile? WorkerProfile { get; set; }
}