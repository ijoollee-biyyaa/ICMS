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

public class ChurchDepartmentService(
    IChurchDepartmentRepository departmentRepository,
    CreateDepartmentValidator createValidator,
    UpdateDepartmentValidator updateValidator,
    AddDepartmentEmployeeValidator addEmployeeValidator,
    ILogger<ChurchDepartmentService> logger) : IChurchDepartmentService
{
    public async Task<Result<DepartmentResponseDto, ChurchDepartmentError>> CreateDepartmentAsync(
        long churchId, CreateDepartmentRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.ChurchNotFound(churchId));

        if (request.HeadEmployeeId is not null
            && !await departmentRepository.HeadIsChurchEmployeeAsync(request.HeadEmployeeId.Value, churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.HeadNotChurchEmployee(request.HeadEmployeeId.Value, churchId));

        var department = new Department
        {
            ChurchId = churchId,
            Name = request.Name!.Trim(),
            Type = request.Type!.Value,
            HeadEmployeeId = request.HeadEmployeeId
        };

        var created = await departmentRepository.AddAsync(department, ct);

        logger.LogInformation("Created church {ChurchId} department {DepartmentId} ({Name})",
            churchId, created.Id, created.Name);

        return Result<DepartmentResponseDto, ChurchDepartmentError>.Success(ToDto(created));
    }

    public async Task<Result<DepartmentResponseDto, ChurchDepartmentError>> GetDepartmentAsync(
        long churchId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.ChurchNotFound(churchId));

        var department = await departmentRepository.GetDepartmentAsync(churchId, departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.NotFound(departmentId));

        return Result<DepartmentResponseDto, ChurchDepartmentError>.Success(
            new DepartmentResponseDto(department.Id, department.Name, department.Type,
                department.HeadEmployeeId, department.HeadEmployeeName, department.EmployeeCount));
    }

    public async Task<Result<DepartmentResponseDto, ChurchDepartmentError>> UpdateDepartmentAsync(
        long churchId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct)
    {
        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.ChurchNotFound(churchId));

        await updateValidator.ValidateAndThrowAsync(request, ct);

        var department = await departmentRepository.GetTrackedDepartmentAsync(churchId, departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.NotFound(departmentId));

        if (request.HeadEmployeeId is not null
            && !await departmentRepository.HeadIsChurchEmployeeAsync(request.HeadEmployeeId.Value, churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.HeadNotChurchEmployee(request.HeadEmployeeId.Value, churchId));

        department.Name = request.Name!.Trim();
        department.Type = request.Type!.Value;
        department.HeadEmployeeId = request.HeadEmployeeId;

        var updated = await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Updated church {ChurchId} department {DepartmentId} ({Name})",
            churchId, updated.Id, updated.Name);

        return Result<DepartmentResponseDto, ChurchDepartmentError>.Success(ToDto(updated));
    }

    public async Task<Result<DepartmentResponseDto, ChurchDepartmentError>> DeleteDepartmentAsync(
        long churchId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.ChurchNotFound(churchId));

        var department = await departmentRepository.GetTrackedDepartmentAsync(churchId, departmentId, ct);
        if (department is null)
            return Result<DepartmentResponseDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.NotFound(departmentId));

        department.IsDeleted = true;

        var deleted = await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Deleted church {ChurchId} department {DepartmentId} ({Name})",
            churchId, deleted.Id, deleted.Name);

        return Result<DepartmentResponseDto, ChurchDepartmentError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<DepartmentResponseDto>, ChurchDepartmentError>> GetDepartmentsAsync(
        long churchId, PagedRequest paging, CancellationToken ct)
    {
        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return Result<PagedResponse<DepartmentResponseDto>, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.ChurchNotFound(churchId));

        var page = paging.SafePage;

        var departments = await departmentRepository.GetDepartmentsAsync(
            churchId, paging.Search, page, paging.PageSize, ct);
        var totalCount = await departmentRepository.CountDepartmentsAsync(churchId, paging.Search, ct);

        return Result<PagedResponse<DepartmentResponseDto>, ChurchDepartmentError>.Success(
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

    public async Task<Result<List<DepartmentEmployeeDto>, ChurchDepartmentError>> GetEmployeesAsync(
        long churchId, long departmentId, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(churchId, departmentId, ct);
        if (guard is not null)
            return Result<List<DepartmentEmployeeDto>, ChurchDepartmentError>.Failure(guard);

        var employees = await departmentRepository.GetEmployeesAsync(departmentId, ct);

        return Result<List<DepartmentEmployeeDto>, ChurchDepartmentError>.Success(
            employees.Select(e => new DepartmentEmployeeDto(e.Id, e.EmployeeId, e.EmployeeName,
                e.EmployeePosition, e.Role)).ToList());
    }

    public async Task<Result<DepartmentEmployeeDto, ChurchDepartmentError>> AssignEmployeeAsync(
        long churchId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(churchId, departmentId, ct);
        if (guard is not null)
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(guard);

        await addEmployeeValidator.ValidateAndThrowAsync(request, ct);

        var employeeId = request.EmployeeId!.Value;

        if (!await departmentRepository.EmployeeInChurchAsync(employeeId, churchId, ct))
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.EmployeeNotOfChurch(employeeId, churchId));

        if (await departmentRepository.GetAssignmentAsync(departmentId, employeeId, ct) is not null)
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.EmployeeAlreadyInDepartment(employeeId, departmentId));

        var role = request.Role!.Value;
        var department = await departmentRepository.GetTrackedDepartmentAsync(churchId, departmentId, ct);

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

            logger.LogInformation("Assigned employee {EmployeeId} to church department {DepartmentId} as {Role}",
                created.EmployeeId, created.DepartmentId, created.Role);

            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Success(
                new DepartmentEmployeeDto(created.Id, created.EmployeeId,
                    FullName(created.Employee.Member), created.Employee.Position, created.Role));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.EmployeeAlreadyInDepartment(employeeId, departmentId));
        }
    }

    public async Task<Result<DepartmentEmployeeDto, ChurchDepartmentError>> RemoveEmployeeAsync(
        long churchId, long departmentId, long departmentEmployeeId, CancellationToken ct)
    {
        var guard = await EnsureDepartmentAsync(churchId, departmentId, ct);
        if (guard is not null)
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(guard);

        var assignment = await departmentRepository.GetAssignmentByIdAsync(
            departmentId, departmentEmployeeId, ct);
        if (assignment is null)
            return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Failure(
                ChurchDepartmentError.DepartmentEmployeeNotFound(departmentEmployeeId));

        var dto = new DepartmentEmployeeDto(assignment.Id, assignment.EmployeeId,
            FullName(assignment.Employee.Member), assignment.Employee.Position, assignment.Role);

        var department = await departmentRepository.GetTrackedDepartmentAsync(churchId, departmentId, ct);
        if (department!.HeadEmployeeId == assignment.EmployeeId)
            department.HeadEmployeeId = null;

        await departmentRepository.RemoveAssignmentAsync(assignment, ct);
        await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Removed employee {EmployeeId} from church department {DepartmentId}",
            dto.EmployeeId, departmentId);

        return Result<DepartmentEmployeeDto, ChurchDepartmentError>.Success(dto);
    }

    private async Task<ChurchDepartmentError?> EnsureDepartmentAsync(
        long churchId, long departmentId, CancellationToken ct)
    {
        if (!await departmentRepository.ChurchExistsAsync(churchId, ct))
            return ChurchDepartmentError.ChurchNotFound(churchId);

        if (await departmentRepository.GetDepartmentAsync(churchId, departmentId, ct) is null)
            return ChurchDepartmentError.NotFound(departmentId);

        return null;
    }

    private static string? FullName(Member? member) =>
        member is null ? null : MemberNames.Full(member);

    private static DepartmentResponseDto ToDto(Department d) =>
        new(d.Id, d.Name, d.Type, d.HeadEmployeeId, null, 0);
}