
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanai3y.Services.Dtos.Workers;
using Sanai3y.Services.Workers;

namespace Sanai3y.Api.Controllers;

[ApiController]
[Route("api/workers")]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;

    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkers(
        [FromQuery] WorkerQueryDto query)
    {
        var result = await _workerService.GetWorkersAsync(query);

        return Ok(result);
    }

    [Authorize(Roles = "Worker")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (workerId is null)
            return Unauthorized();

        var profile = await _workerService.GetProfileAsync(workerId);

        if (profile is null)
            return NotFound(new { message = "Worker profile not found." });

        return Ok(profile);
    }
}
