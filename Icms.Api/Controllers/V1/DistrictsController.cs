using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/districts")]
[Tags("Districts")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DistrictsController(
    IDistrictService districtService,
    LinkGenerator linkGenerator,
    IAuthorizationService authorizationService,
    IcmsDbContext db) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a district")]
    [EndpointDescription("Creates a district (the district office's own record). The code is normalized to upper case. Returns 409 if the code is already used. Admin only.")]
    public async Task<IActionResult> CreateDistrict(
        [FromBody] CreateDistrictRequest request, CancellationToken ct)
    {
        var result = await districtService.CreateDistrictAsync(request, ct);

        return result.Match<IActionResult>(
            district => CreatedAtAction(nameof(GetDistrictById), new { id = district.Id },
                district with { Links = BuildLinks(district.Id) }),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DistrictResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List districts")]
    [EndpointDescription("Paged list of districts, searchable by name or code.")]
    public async Task<IActionResult> GetDistricts(
        [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var page = await districtService.GetDistrictsAsync(paging, ct);

        return Ok(page);
    }

    [HttpGet("{id:long}", Name = nameof(GetDistrictById))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get district by ID")]
    [EndpointDescription("Returns the district with its church and member counts. Public (used by the landing page).")]
    public async Task<IActionResult> GetDistrictById(long id, CancellationToken ct)
    {
        var result = await districtService.GetDistrictAsync(id, ct);

        return result.Match<IActionResult>(
            district => Ok(district with { Links = BuildLinks(district.Id) }),
            error => error.ToResult(Request));
    }

    [HttpPut("{id:long}", Name = nameof(UpdateDistrict))]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a district")]
    [EndpointDescription("Updates the editable fields of a district. Returns 409 if the new code belongs to another district. Admin or the district's president.")]
    public async Task<IActionResult> UpdateDistrict(
        long id, UpdateDistrictRequest request, CancellationToken ct)
    {
        var district = await db.Districts.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (district == null)
        {
            return NotFound(new ProblemDetails { Title = "District not found." });
        }

        var authResult = await authorizationService.AuthorizeAsync(User, district, "CanManageDistrict");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var result = await districtService.UpdateDistrictAsync(id, request, ct);

        return result.Match<IActionResult>(
            updated => Ok(updated with { Links = BuildLinks(updated.Id) }),
            error => error.ToResult(Request));
    }

    [HttpDelete("{id:long}", Name = nameof(DeleteDistrict))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete a district")]
    [EndpointDescription("Soft-deletes a district. Returns 409 if the district still has churches. Admin or the district's president.")]
    public async Task<IActionResult> DeleteDistrict(long id, CancellationToken ct)
    {
        var district = await db.Districts.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (district == null)
        {
            return NotFound(new ProblemDetails { Title = "District not found." });
        }

        var authResult = await authorizationService.AuthorizeAsync(User, district, "CanManageDistrict");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var result = await districtService.DeleteDistrictAsync(id, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }

    private IReadOnlyList<LinkDto> BuildLinks(long id)
    {
        var path = (string routeName, object values) =>
            linkGenerator.GetPathByName(HttpContext, routeName, values)!;

        return
        [
            new(path(nameof(GetDistrictById), new { id }), "self", "GET"),
            new(path(nameof(UpdateDistrict), new { id }), "update", "PUT"),
            new(path(nameof(DeleteDistrict), new { id }), "delete", "DELETE")
        ];
    }
}