
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
}
