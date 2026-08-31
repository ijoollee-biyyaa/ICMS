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

public class ChurchEmployeeService(
    IChurchEmployeeRepository employeeRepository,
    CreateEmployeeValidator createValidator,
    UpdateEmployeeValidator updateValidator,
    ILogger<ChurchEmployeeService> logger) : IChurchEmployeeService
{
    public async Task<Result<EmployeeResponseDto, ChurchEmployeeError>> CreateEmployeeAsync(
        long churchId, CreateEmployeeRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        if (!await employeeRepository.ChurchExistsAsync(churchId, ct))
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.ChurchNotFound(churchId));

        var employmentType = request.EmploymentType!.Value;

        if (employmentType == EmploymentType.FulltimeMinister && request.MemberId is null)
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.MinisterRequiresMember());

        if (request.MemberId is not null
            && await employeeRepository.GetMemberInChurchAsync(churchId, request.MemberId.Value, ct) is null)
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.MemberNotFound(request.MemberId.Value));

        var salaryPaidBy = request.MemberId is { } memberId
            && await employeeRepository.MemberIsDistrictEmployeeAsync(memberId, ct)
            ? SalaryPaidBy.District
            : SalaryPaidBy.Church;

        // Reuse the member's existing login account (member credentials or another employment),
        // so the employee shows up on the Accounts page and can be granted church roles.
        string? accountUserId = request.MemberId is { } hiredMemberId
            ? await employeeRepository.FindMemberAccountUserIdAsync(hiredMemberId, ct)
            : null;

        var employee = new Employee
        {
            ChurchId = churchId,
            Position = request.Position!.Trim(),
            EmploymentType = employmentType,
            MinisterTitle = request.MinisterTitle,
            MemberId = request.MemberId,
            UserId = accountUserId,
            FirstName = request.MemberId is null ? request.FirstName?.Trim() : null,
            FatherName = request.MemberId is null ? request.FatherName?.Trim() : null,
            GrandfatherName = request.MemberId is null ? request.GrandfatherName?.Trim() : null,
            Salary = request.Salary,
            SalaryPaidBy = salaryPaidBy,
            HireDate = request.HireDate,
            Status = EmployeeStatus.Active
        };

        try
        {
            var created = await employeeRepository.AddAsync(employee, ct);

            logger.LogInformation("Created church {ChurchId} employee {EmployeeId} ({Position}, {EmploymentType})",
                churchId, created.Id, created.Position, created.EmploymentType);

            var dto = ToDto(created, accountNames: (request.FirstName, request.FatherName, request.GrandfatherName));

            return Result<EmployeeResponseDto, ChurchEmployeeError>.Success(dto);
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.MemberAlreadyHired(request.MemberId!.Value, churchId));
        }
    }

    public async Task<Result<EmployeeResponseDto, ChurchEmployeeError>> GetEmployeeAsync(
        long churchId, long employeeId, CancellationToken ct)
    {
        if (!await employeeRepository.ChurchExistsAsync(churchId, ct))
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.ChurchNotFound(churchId));

        var employee = await employeeRepository.GetEmployeeAsync(churchId, employeeId, ct);

        return employee is null
            ? Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.NotFound(employeeId))
            : Result<EmployeeResponseDto, ChurchEmployeeError>.Success(employee.ToDto());
    }

    public async Task<Result<EmployeeResponseDto, ChurchEmployeeError>> UpdateEmployeeAsync(
        long churchId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct)
    {
        if (!await employeeRepository.ChurchExistsAsync(churchId, ct))
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.ChurchNotFound(churchId));

        await updateValidator.ValidateAndThrowAsync(request, ct);

        var employee = await employeeRepository.GetTrackedEmployeeAsync(churchId, employeeId, ct);
        if (employee is null)
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.NotFound(employeeId));

        employee.Position = request.Position!.Trim();
        employee.MinisterTitle = request.MinisterTitle;
        employee.Salary = request.Salary;
        employee.HireDate = request.HireDate;
        employee.Status = request.Status ?? employee.Status;

        var updated = await employeeRepository.UpdateAsync(employee, ct);

        logger.LogInformation("Updated church {ChurchId} employee {EmployeeId} ({Position})",
            churchId, updated.Id, updated.Position);

        return Result<EmployeeResponseDto, ChurchEmployeeError>.Success(ToDto(updated));
    }

    public async Task<Result<EmployeeResponseDto, ChurchEmployeeError>> DeleteEmployeeAsync(
        long churchId, long employeeId, CancellationToken ct)
    {
        if (!await employeeRepository.ChurchExistsAsync(churchId, ct))
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.ChurchNotFound(churchId));

        var employee = await employeeRepository.GetTrackedEmployeeAsync(churchId, employeeId, ct);
        if (employee is null)
            return Result<EmployeeResponseDto, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.NotFound(employeeId));

        employee.IsDeleted = true;

        var deleted = await employeeRepository.UpdateAsync(employee, ct);

        logger.LogInformation("Deleted church {ChurchId} employee {EmployeeId} ({Position})",
            churchId, deleted.Id, deleted.Position);

        return Result<EmployeeResponseDto, ChurchEmployeeError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<EmployeeResponseDto>, ChurchEmployeeError>> GetEmployeesAsync(
        long churchId, PagedRequest paging, CancellationToken ct)
    {
        if (!await employeeRepository.ChurchExistsAsync(churchId, ct))
            return Result<PagedResponse<EmployeeResponseDto>, ChurchEmployeeError>.Failure(
                ChurchEmployeeError.ChurchNotFound(churchId));

        var page = paging.SafePage;

        var employees = await employeeRepository.GetEmployeesAsync(
            churchId, paging.Search, page, paging.PageSize, ct);
        var totalCount = await employeeRepository.CountEmployeesAsync(churchId, paging.Search, ct);

        return Result<PagedResponse<EmployeeResponseDto>, ChurchEmployeeError>.Success(
            new PagedResponse<EmployeeResponseDto>
            {
                Items = employees.Select(e => e.ToDto()).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    private static EmployeeResponseDto ToDto(Employee e,
        (string?, string?, string?)? accountNames = null)
    {
        var memberName = e.Member is null ? null : MemberNames.Full(e.Member);
        if (memberName is null)
        {
            var stored = string.Join(" ",
                new[] { e.FirstName, e.FatherName, e.GrandfatherName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(stored) && accountNames is { } names)
            {
                stored = string.Join(" ",
                    new[] { names.Item1, names.Item2, names.Item3 }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            }
            memberName = string.IsNullOrWhiteSpace(stored) ? null : stored;
        }

        return new(e.Id, e.Position, e.EmploymentType, e.MinisterTitle, e.MemberId,
            memberName, e.Member?.EfgbcId, e.Member?.ChurchId, e.Member?.Church?.Name,
            e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate,
            e.Status, e.IsDistrictPresident, e.IsVicePresident, null, null);
    }
}