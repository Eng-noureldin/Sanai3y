using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sanai3y.Services.Dtos.Requests;

public class CreateRequestDto
{
    [Required]
    public int ProfessionId { get; set; }

    [Required]
    public int DistrictId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public IFormFileCollection? Photos { get; set; }
}