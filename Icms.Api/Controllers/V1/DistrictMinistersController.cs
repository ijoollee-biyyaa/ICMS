using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/districts/{districtId:long}/ministers")]
[Tags("District Office")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DistrictMinistersController(IDistrictEmployeeService employeeService) : ControllerBase
{
    [HttpGet(Name = nameof(GetMinisters))]
    [ProducesResponseType(typeof(PagedResponse<DistrictMinisterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List district full-time ministers")]
    [EndpointDescription("All full-time ministers across the district's churches and the office, one row per person (no duplicates when a minister also works at the district office). Each row lists his placements with the paid-by badge.")]
    public async Task<IActionResult> GetMinisters(
        long districtId, [FromQuery] PagedRequest paging, [FromQuery] string? search, CancellationToken ct)
    {
        var result = await employeeService.GetMinistersAsync(
            districtId, search, paging.Page, paging.PageSize, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }
}
