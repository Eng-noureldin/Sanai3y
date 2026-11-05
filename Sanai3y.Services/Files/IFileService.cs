using Microsoft.AspNetCore.Http;

namespace Sanai3y.Services.Files;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile file, bool isPrivate);
    Task<(Stream FileStream, string ContentType)> OpenPrivateAsync(string relativePath);
}