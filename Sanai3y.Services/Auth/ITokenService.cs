using Sanai3y.Data.Entities;

namespace Sanai3y.Services.Auth;

public interface ITokenService
{
    string CreateToken(User user);
}