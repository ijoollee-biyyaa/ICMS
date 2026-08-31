using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/districts/{districtId:long}/employees")]
[Tags("District Office")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DistrictEmployeesController(IDistrictEmployeeService employeeService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Hire a district office employee")]
    [EndpointDescription("Hires an office employee. Full-time ministers must be linked to a church member; hired staff (any faith) need no member link. The president and vice president must be full-time ministers of a local church, one each.")]
    public async Task<IActionResult> CreateEmployee(
        long districtId, CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.CreateEmployeeAsync(districtId, request, ct);

        return result.Match<IActionResult>(
            employee => CreatedAtAction(nameof(GetEmployee),
                new { districtId, employeeId = employee.Id }, employee),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EmployeeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List office employees")]
    public async Task<IActionResult> GetEmployees(
        long districtId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await employeeService.GetEmployeesAsync(districtId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("executives")]
    [ProducesResponseType(typeof(OfficeExecutivesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Office executives")]
    [EndpointDescription("The district president and vice president.")]
    public async Task<IActionResult> GetExecutives(long districtId, CancellationToken ct)
    {
        var result = await employeeService.GetExecutivesAsync(districtId, ct);

        return result.Match<IActionResult>(
            executives => Ok(executives),
            error => error.ToResult(Request));
    }

    [HttpGet("{employeeId:long}", Name = nameof(GetEmployee))]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get an office employee")]
    public async Task<IActionResult> GetEmployee(
        long districtId, long employeeId, CancellationToken ct)
    {
        var result = await employeeService.GetEmployeeAsync(districtId, employeeId, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }

    [HttpPut("{employeeId:long}", Name = nameof(UpdateEmployee))]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an office employee")]
    [EndpointDescription("Updates position, title, salary, status and executive flags. The member link and employment type are fixed at hiring.")]
    public async Task<IActionResult> UpdateEmployee(
        long districtId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.UpdateEmployeeAsync(districtId, employeeId, request, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }

    [HttpDelete("{employeeId:long}", Name = nameof(DeleteEmployee))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an office employee")]
    public async Task<IActionResult> DeleteEmployee(
        long districtId, long employeeId, CancellationToken ct)
    {
        var result = await employeeService.DeleteEmployeeAsync(districtId, employeeId, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }
}