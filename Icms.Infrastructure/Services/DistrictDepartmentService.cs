using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.Districts;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Services;

public class DistrictDepartmentService(
    IDistrictDepartmentRepository departmentRepository,
    CreateDepartmentValidator createValidator,
    UpdateDepartmentValidator updateValidator,
    AddDepartmentEmployeeValidator addEmployeeValidator,
    ILogger<DistrictDepartmentService> logger) : IDistrictDepartmentService
{
    public async Task<Result<DepartmentResponseDto, DistrictDepartmentError>> CreateDepartmentAsync(
        long districtId, CreateDepartmentRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DistrictNotFound(districtId));

        if (request.HeadEmployeeId is not null
            && !await departmentRepository.HeadIsOfficeEmployeeAsync(request.HeadEmployeeId.Value, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.HeadNotOfficeEmployee(request.HeadEmployeeId.Value));

        var department = new Department
        {
            ChurchId = null,
            Name = request.Name!.Trim(),
            Type = request.Type!.Value,
            HeadEmployeeId = request.HeadEmployeeId
        };

        var created = await departmentRepository.AddAsync(department, ct);

        logger.LogInformation("Created office department {DepartmentId} ({Name})",
            created.Id, created.Name);

        return Result<DepartmentResponseDto, DistrictDepartmentError>.Success(ToDto(created));
    }

    public async Task<Result<DepartmentResponseDto, DistrictDepartmentError>> GetDepartmentAsync(
        long districtId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DistrictNotFound(districtId));

        var department = await departmentRepository.GetDepartmentAsync(departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.NotFound(departmentId));

        return Result<DepartmentResponseDto, DistrictDepartmentError>.Success(
            new DepartmentResponseDto(department.Id, department.Name, department.Type,
                department.HeadEmployeeId, department.HeadEmployeeName, department.EmployeeCount));
    }

    public async Task<Result<DepartmentResponseDto, DistrictDepartmentError>> UpdateDepartmentAsync(
        long districtId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct)
    {
        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DistrictNotFound(districtId));

        await updateValidator.ValidateAndThrowAsync(request, ct);

        var department = await departmentRepository.GetTrackedDepartmentAsync(departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.NotFound(departmentId));

        if (request.HeadEmployeeId is not null
            && !await departmentRepository.HeadIsOfficeEmployeeAsync(request.HeadEmployeeId.Value, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.HeadNotOfficeEmployee(request.HeadEmployeeId.Value));

        department.Name = request.Name!.Trim();
        department.Type = request.Type!.Value;
        department.HeadEmployeeId = request.HeadEmployeeId;

        var updated = await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Updated office department {DepartmentId} ({Name})",
            updated.Id, updated.Name);

        return Result<DepartmentResponseDto, DistrictDepartmentError>.Success(ToDto(updated));
    }

    public async Task<Result<DepartmentResponseDto, DistrictDepartmentError>> DeleteDepartmentAsync(
        long districtId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DistrictNotFound(districtId));

        var department = await departmentRepository.GetTrackedDepartmentAsync(departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.NotFound(departmentId));

        department.IsDeleted = true;

        var deleted = await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Deleted office department {DepartmentId} ({Name})",
            deleted.Id, deleted.Name);

        return Result<DepartmentResponseDto, DistrictDepartmentError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<DepartmentResponseDto>, DistrictDepartmentError>> GetDepartmentsAsync(
        long districtId, PagedRequest paging, CancellationToken ct)
    {
        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return Result<PagedResponse<DepartmentResponseDto>, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DistrictNotFound(districtId));

        var page = paging.SafePage;

        var departments = await departmentRepository.GetDepartmentsAsync(
            paging.Search, page, paging.PageSize, ct);
        var totalCount = await departmentRepository.CountDepartmentsAsync(paging.Search, ct);

        return Result<PagedResponse<DepartmentResponseDto>, DistrictDepartmentError>.Success(
            new PagedResponse<DepartmentResponseDto>
            {
                Items = departments
                    .Select(d => new DepartmentResponseDto(d.Id, d.Name, d.Type,
                        d.HeadEmployeeId, d.HeadEmployeeName, d.EmployeeCount))
                    .ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<List<DepartmentEmployeeDto>, DistrictDepartmentError>> GetEmployeesAsync(
        long districtId, long departmentId, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(districtId, departmentId, ct);
        if (guard is not null)
            return Result<List<DepartmentEmployeeDto>, DistrictDepartmentError>.Failure(guard);

        var employees = await departmentRepository.GetEmployeesAsync(departmentId, ct);

        return Result<List<DepartmentEmployeeDto>, DistrictDepartmentError>.Success(
            employees.Select(e => new DepartmentEmployeeDto(e.Id, e.EmployeeId, e.EmployeeName,
                e.EmployeePosition, e.Role)).ToList());
    }

    public async Task<Result<DepartmentEmployeeDto, DistrictDepartmentError>> AssignEmployeeAsync(
        long districtId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(districtId, departmentId, ct);
        if (guard is not null)
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(guard);

        await addEmployeeValidator.ValidateAndThrowAsync(request, ct);

        var employeeId = request.EmployeeId!.Value;

        if (!await departmentRepository.EmployeeIsOfficeEmployeeAsync(employeeId, ct))
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.EmployeeNotOfficeEmployee(employeeId));

        if (await departmentRepository.GetAssignmentAsync(departmentId, employeeId, ct) is not null)
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.EmployeeAlreadyInDepartment(employeeId, departmentId));

        var role = request.Role!.Value;
        var department = await departmentRepository.GetTrackedDepartmentAsync(departmentId, ct);

        if (role == DepartmentEmployeeRole.Head)
            department!.HeadEmployeeId = employeeId;

        var assignment = new DepartmentEmployee
        {
            DepartmentId = departmentId,
            EmployeeId = employeeId,
            Role = role
        };

        try
        {
            var created = await departmentRepository.AssignAsync(assignment, ct);
            await departmentRepository.UpdateAsync(department!, ct);

            logger.LogInformation("Assigned employee {EmployeeId} to department {DepartmentId} as {Role}",
                created.EmployeeId, created.DepartmentId, created.Role);

            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Success(
                new DepartmentEmployeeDto(created.Id, created.EmployeeId,
                    FullName(created.Employee.Member), created.Employee.Position, created.Role));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.EmployeeAlreadyInDepartment(employeeId, departmentId));
        }
    }

    public async Task<Result<DepartmentEmployeeDto, DistrictDepartmentError>> RemoveEmployeeAsync(
        long districtId, long departmentId, long departmentEmployeeId, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(districtId, departmentId, ct);
        if (guard is not null)
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(guard);

        var assignment = await departmentRepository.GetAssignmentByIdAsync(
            departmentId, departmentEmployeeId, ct);
        if (assignment is null)
            return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Failure(
                DistrictDepartmentError.DepartmentEmployeeNotFound(departmentEmployeeId));

        var dto = new DepartmentEmployeeDto(assignment.Id, assignment.EmployeeId,
            FullName(assignment.Employee.Member), assignment.Employee.Position, assignment.Role);

        var department = await departmentRepository.GetTrackedDepartmentAsync(departmentId, ct);
        if (department!.HeadEmployeeId == assignment.EmployeeId)
            department.HeadEmployeeId = null;

        await departmentRepository.RemoveAssignmentAsync(assignment, ct);
        await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Removed employee {EmployeeId} from department {DepartmentId}",
            dto.EmployeeId, departmentId);

        return Result<DepartmentEmployeeDto, DistrictDepartmentError>.Success(dto);
    }

    private async Task<DistrictDepartmentError?> EnsureDepartmentAsync(
        long districtId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.DistrictExistsAsync(districtId, ct))
            return DistrictDepartmentError.DistrictNotFound(districtId);

        if (await departmentRepository.GetDepartmentAsync(departmentId, ct) is null)
            return DistrictDepartmentError.NotFound(departmentId);

        return null;
    }

    private static string? FullName(Member? member) =>
        member is null ? null : MemberNames.Full(member);

    private static DepartmentResponseDto ToDto(Department d) =>
        new(d.Id, d.Name, d.Type, d.HeadEmployeeId, null, 0);
}
