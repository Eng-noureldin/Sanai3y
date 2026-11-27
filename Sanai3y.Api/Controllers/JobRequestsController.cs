using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanai3y.Services.Dtos.Requests;
using Sanai3y.Services.Requests;

namespace Sanai3y.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class JobRequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public JobRequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateRequestDto dto)
    {
        var id = await _requestService.CreateAsync(UserId, dto);
        return Ok(new { id });
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine(int page = 1, int pageSize = 10)
    {
        return Ok(await _requestService.GetMyRequestsAsync(UserId, page, pageSize));
    }

    [HttpGet("nearby")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetNearby(int page = 1, int pageSize = 10)
    {
        return Ok(await _requestService.GetNearbyForWorkerAsync(UserId, page, pageSize));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetails(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await _requestService.GetDetailsAsync(id, userId));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id, [FromQuery] string? reason)
    {
        await _requestService.CancelAsync(id, UserId, reason);
        return NoContent();
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Complete(int id)
    {
        await _requestService.CompleteAsync(id, UserId);
        return NoContent();
    }
}