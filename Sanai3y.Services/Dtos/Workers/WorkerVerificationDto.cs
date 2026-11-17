using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Sanai3y.Services.Dtos.Workers;

public class WorkerVerificationDto
{
    [Required]
    public IFormFile IdPhoto { get; set; } = null!;

    [Required]
    public IFormFile Selfie { get; set; } = null!;
}