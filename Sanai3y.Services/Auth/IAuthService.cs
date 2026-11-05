using Sanai3y.Services.Dtos.Auth;

namespace Sanai3y.Services.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
}