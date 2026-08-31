using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/departments")]
[Tags("Church Office")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ChurchDepartmentsController(IChurchDepartmentService departmentService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a church department")]
    [EndpointDescription("Creates a church department (e.g. Sunday School, Media). The head must be an employee of this church.")]
    public async Task<IActionResult> CreateDepartment(
        long churchId, CreateDepartmentRequest request, CancellationToken ct)
    {
        var result = await departmentService.CreateDepartmentAsync(churchId, request, ct);

        return result.Match<IActionResult>(
            department => CreatedAtAction(nameof(GetDepartment),
                new { churchId, departmentId = department.Id }, department),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List church departments")]
    public async Task<IActionResult> GetDepartments(
        long churchId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await departmentService.GetDepartmentsAsync(churchId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("{departmentId:long}", Name = "ChurchGetDepartment")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a church department")]
    public async Task<IActionResult> GetDepartment(
        long churchId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.GetDepartmentAsync(churchId, departmentId, ct);

        return result.Match<IActionResult>(
            department => Ok(department),
            error => error.ToResult(Request));
    }

    [HttpPut("{departmentId:long}", Name = "ChurchUpdateDepartment")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a church department")]
    public async Task<IActionResult> UpdateDepartment(
        long churchId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct)
    {
        var result = await departmentService.UpdateDepartmentAsync(churchId, departmentId, request, ct);

        return result.Match<IActionResult>(
            department => Ok(department),
            error => error.ToResult(Request));
    }

    [HttpDelete("{departmentId:long}", Name = "ChurchDeleteDepartment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a church department")]
    public async Task<IActionResult> DeleteDepartment(
        long churchId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.DeleteDepartmentAsync(churchId, departmentId, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }

    [HttpGet("{departmentId:long}/employees")]
    [ProducesResponseType(typeof(List<DepartmentEmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List department employees")]
    [EndpointDescription("Lists the church employees assigned to the department, with their role (Head, Staff or Secretary).")]
    public async Task<IActionResult> GetEmployees(
        long churchId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.GetEmployeesAsync(churchId, departmentId, ct);

        return result.Match<IActionResult>(
            employees => Ok(employees),
            error => error.ToResult(Request));
    }

    [HttpPost("{departmentId:long}/employees")]
    [ProducesResponseType(typeof(DepartmentEmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Assign a church employee to a department")]
    [EndpointDescription("Assigns an employee of this church to the department with a role. Assigning as Head also makes the employee the department head. One assignment per employee per department; duplicates return 409.")]
    public async Task<IActionResult> AssignEmployee(
        long churchId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct)
    {
        var result = await departmentService.AssignEmployeeAsync(churchId, departmentId, request, ct);

        return result.Match<IActionResult>(
            employee => CreatedAtAction(nameof(GetEmployees), new { churchId, departmentId }, employee),
            error => error.ToResult(Request));
    }

    [HttpDelete("{departmentId:long}/employees/{departmentEmployeeId:long}")]
    [ProducesResponseType(typeof(DepartmentEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Remove an employee from a department")]
    public async Task<IActionResult> RemoveEmployee(
        long churchId, long departmentId, long departmentEmployeeId, CancellationToken ct)
    {
        var result = await departmentService.RemoveEmployeeAsync(
            churchId, departmentId, departmentEmployeeId, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }
}