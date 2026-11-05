using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Sanai3y.Services.Common;

namespace Sanai3y.Services.Files;

public class FileService : IFileService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB limit
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(IFormFile file, bool isPrivate)
    {
        if (file == null || file.Length == 0)
            throw new BusinessException("No file was provided.");

        if (file.Length > MaxFileSizeBytes)
            throw new BusinessException("File size exceeds the 5 MB limit.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new BusinessException("Invalid file extension. Only .jpg, .jpeg, and .png are allowed.");

        var newFileName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";

        // Determine destination folder
        string targetFolder = isPrivate
            ? Path.Combine(_env.ContentRootPath, "PrivateUploads")
            : Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");

        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var destinationFilePath = Path.Combine(targetFolder, newFileName);

        using (var stream = new FileStream(destinationFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path or public URI identifier
        return isPrivate ? newFileName : $"/uploads/{newFileName}";
    }

    public Task<(Stream FileStream, string ContentType)> OpenPrivateAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new BusinessException("Invalid file name.");

        var safeFileName = Path.GetFileName(relativePath);
        var fullPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", safeFileName);

        if (!File.Exists(fullPath))
            throw new BusinessException("Document not found.");

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".png" => "image/png",
            _ => "image/jpeg"
        };

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult((stream, contentType));
    }
}