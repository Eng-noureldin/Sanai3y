using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sanai3y.Data;
using Sanai3y.Data.Entities;
using Sanai3y.Data.Enums;
using Sanai3y.Services.Common;
using Sanai3y.Services.Dtos.Auth;

namespace Sanai3y.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<User> userManager, AppDbContext db, ITokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        if (dto.Role != UserRole.Customer && dto.Role != UserRole.Worker)
            throw new BusinessException("Cannot register with this role.");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new BusinessException("Email is already registered.");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessException(errors);
        }

        await _userManager.AddToRoleAsync(user, dto.Role.ToString());

        // Create empty WorkerProfile shell if registering as Worker
        if (dto.Role == UserRole.Worker)
        {
            // Pick default city for initial profile creation
            var defaultCity = await _db.Cities.FirstOrDefaultAsync(c => c.IsActive);
            if (defaultCity == null)
                throw new BusinessException("System lookups not initialized.");

            var profile = new WorkerProfile
            {
                UserId = user.Id,
                CityId = defaultCity.Id,
                Bio = string.Empty,
                ExperienceYears = 0,
                IsAvailable = true,
                VerificationStatus = VerificationStatus.NotSubmitted,
                AvgRating = 0.0m,
                RatingCount = 0,
                CompletedJobsCount = 0
            };

            await _db.WorkerProfiles.AddAsync(profile);
            await _db.SaveChangesAsync();
        }

        var token = _tokenService.CreateToken(user);

        return new AuthResultDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new BusinessException("Invalid email or password.");

        // Rule 7: Blocked users cannot log in
        if (user.IsBlocked)
            throw new BusinessException("This account has been suspended.");

        var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
            throw new BusinessException("Invalid email or password.");

        var token = _tokenService.CreateToken(user);

        return new AuthResultDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}