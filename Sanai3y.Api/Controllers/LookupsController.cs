using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanai3y.Services.Dtos.Lookups;
using Sanai3y.Services.Lookups;

namespace Sanai3y.Api.Controllers;

[ApiController]
[Route("api/lookups")]
[AllowAnonymous]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetCities()
    {
        var cities = await _lookupService.GetCitiesAsync();
        return Ok(cities);
    }

    [HttpGet("cities/{cityId:int}/districts")]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetDistricts(int cityId)
    {
        var districts = await _lookupService.GetDistrictsAsync(cityId);
        return Ok(districts);
    }

    [HttpGet("professions")]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetProfessions()
    {
        var professions = await _lookupService.GetProfessionsAsync();
        return Ok(professions);
    }
}