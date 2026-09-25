using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize(Roles = "Admin,DistrictSubAdmin")]
[ApiController]
[Route("api/districts/{districtId:long}/departments")]
[Tags("District Office")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DistrictDepartmentsController(IDistrictDepartmentService departmentService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create an office department")]
    [EndpointDescription("Creates a district office department (gospel or administrative, e.g. Bible Mission, Youth, Training & Education, HR, Finance, Media). The head must be a district office employee.")]
    public async Task<IActionResult> CreateDepartment(
        long districtId, CreateDepartmentRequest request, CancellationToken ct)
    {
        var result = await departmentService.CreateDepartmentAsync(districtId, request, ct);

        return result.Match<IActionResult>(
            department => CreatedAtAction(nameof(GetDepartment),
                new { districtId, departmentId = department.Id }, department),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List office departments")]
    public async Task<IActionResult> GetDepartments(
        long districtId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await departmentService.GetDepartmentsAsync(districtId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("{departmentId:long}", Name = nameof(GetDepartment))]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get an office department")]
    public async Task<IActionResult> GetDepartment(
        long districtId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.GetDepartmentAsync(districtId, departmentId, ct);

        return result.Match<IActionResult>(
            department => Ok(department),
            error => error.ToResult(Request));
    }

    [HttpPut("{departmentId:long}", Name = nameof(UpdateDepartment))]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an office department")]
    public async Task<IActionResult> UpdateDepartment(
        long districtId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct)
    {
        var result = await departmentService.UpdateDepartmentAsync(districtId, departmentId, request, ct);

        return result.Match<IActionResult>(
            department => Ok(department),
            error => error.ToResult(Request));
    }

    [HttpDelete("{departmentId:long}", Name = nameof(DeleteDepartment))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an office department")]
    public async Task<IActionResult> DeleteDepartment(
        long districtId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.DeleteDepartmentAsync(districtId, departmentId, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }

    [HttpGet("{departmentId:long}/employees")]
    [ProducesResponseType(typeof(List<DepartmentEmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List department employees")]
    [EndpointDescription("Lists the office employees assigned to the department, with their role (Head, Staff or Secretary).")]
    public async Task<IActionResult> GetEmployees(
        long districtId, long departmentId, CancellationToken ct)
    {
        var result = await departmentService.GetEmployeesAsync(districtId, departmentId, ct);

        return result.Match<IActionResult>(
            employees => Ok(employees),
            error => error.ToResult(Request));
    }

    [HttpPost("{departmentId:long}/employees")]
    [ProducesResponseType(typeof(DepartmentEmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Assign an office employee to a department")]
    [EndpointDescription("Assigns a district office employee to the department with a role. Assigning as Head also makes the employee the department head. One assignment per employee per department; duplicates return 409.")]
    public async Task<IActionResult> AssignEmployee(
        long districtId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct)
    {
        var result = await departmentService.AssignEmployeeAsync(districtId, departmentId, request, ct);

        return result.Match<IActionResult>(
            employee => CreatedAtAction(nameof(GetEmployees), new { districtId, departmentId }, employee),
            error => error.ToResult(Request));
    }

    [HttpDelete("{departmentId:long}/employees/{departmentEmployeeId:long}")]
    [ProducesResponseType(typeof(DepartmentEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Remove an employee from a department")]
    public async Task<IActionResult> RemoveEmployee(
        long districtId, long departmentId, long departmentEmployeeId, CancellationToken ct)
    {
        var result = await departmentService.RemoveEmployeeAsync(
            districtId, departmentId, departmentEmployeeId, ct);

        return result.Match<IActionResult>(
            employee => Ok(employee),
            error => error.ToResult(Request));
    }
}
