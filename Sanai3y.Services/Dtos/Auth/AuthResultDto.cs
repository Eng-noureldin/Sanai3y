using Sanai3y.Data.Enums;

namespace Sanai3y.Services.Dtos.Auth;

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}