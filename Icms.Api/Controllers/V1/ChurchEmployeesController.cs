using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/employees")]
[Tags("Church Office")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ChurchEmployeesController(IChurchEmployeeService employeeService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Hire a church employee")]
    [EndpointDescription("Hires an employee for the church: full-time ministers (must be linked to a member of this church) or staff hired from outside (e.g. security, auditor, cashier) with no member link. One employment per member per church.")]
    public async Task<IActionResult> CreateEmployee(
        long churchId, CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.CreateEmployeeAsync(churchId, request, ct);

        return result.Match<IActionResult>(
            employee => CreatedAtAction(nameof(GetEmployee),
                new { churchId, employeeId = employee.Id }, employee),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EmployeeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List church employees")]
    public async Task<IActionResult> GetEmployees(
        long churchId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await employeeService.GetEmployeesAsync(churchId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("{employeeId:long}", Name = "ChurchGetEmployee")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a church employee")]
    public async Task<IActionResult> GetEmployee(
        long churchId, long employeeId, CancellationToken ct)
    {
        var result = await employeeService.GetEmployeeAsync(churchId, employeeId, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }

    [HttpPut("{employeeId:long}", Name = "ChurchUpdateEmployee")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a church employee")]
    public async Task<IActionResult> UpdateEmployee(
        long churchId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.UpdateEmployeeAsync(churchId, employeeId, request, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }

    [HttpDelete("{employeeId:long}", Name = "ChurchDeleteEmployee")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a church employee")]
    [EndpointDescription("Soft-deletes the employee; deleted employees no longer appear in lists.")]
    public async Task<IActionResult> DeleteEmployee(
        long churchId, long employeeId, CancellationToken ct)
    {
        var result = await employeeService.DeleteEmployeeAsync(churchId, employeeId, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }
}