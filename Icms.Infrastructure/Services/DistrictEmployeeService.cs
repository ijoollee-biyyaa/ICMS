using System.Security.Cryptography;

using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.Districts;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Infrastructure.Services;

public class DistrictEmployeeService(
    IDistrictEmployeeRepository employeeRepository,
    CreateEmployeeValidator createValidator,
    UpdateEmployeeValidator updateValidator,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IcmsDbContext dbContext,
    ILogger<DistrictEmployeeService> logger) : IDistrictEmployeeService
{
   public async Task<Result<EmployeeResponseDto, DistrictEmployeeError>> CreateEmployeeAsync(
    long districtId, CreateEmployeeRequest request, CancellationToken ct)
{
    await createValidator.ValidateAndThrowAsync(request, ct);

    if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
        return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
            DistrictEmployeeError.DistrictNotFound(districtId));

    var districtCode = await dbContext.Districts.AsNoTracking()
        .Where(d => d.Id == districtId)
        .Select(d => d.Code)
        .SingleAsync(ct);

    var employmentType = request.EmploymentType!.Value;

    if (employmentType == EmploymentType.FulltimeMinister && request.MemberId is null)
        return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
            DistrictEmployeeError.MinisterRequiresMember());

    MemberRow? member = null;
    if (request.MemberId is not null)
    {
        member = await employeeRepository.GetMemberInDistrictAsync(districtId, request.MemberId.Value, ct);
        if (member is null)
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.MemberNotFound(request.MemberId.Value));

        if (request.IsDistrictPresident || request.IsVicePresident)
        {
            if (employmentType != EmploymentType.FulltimeMinister)
                return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                    request.IsDistrictPresident
                        ? DistrictEmployeeError.PresidentMustBeMinister()
                        : DistrictEmployeeError.VicePresidentMustBeMinister());

            if (member.ChurchType != ChurchType.Local)
                return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                    DistrictEmployeeError.MemberNotLocalChurch(member.Id));
        }
    }
    else if (request.IsDistrictPresident || request.IsVicePresident)
    {
        return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
            request.IsDistrictPresident
                ? DistrictEmployeeError.PresidentMustBeMinister()
                : DistrictEmployeeError.VicePresidentMustBeMinister());
    }

    if (request.IsDistrictPresident
        && await employeeRepository.GetPresidentAsync(ct) is { } existing)
        return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
            DistrictEmployeeError.PresidentExists(existing.Id));

    if (request.IsVicePresident
        && await employeeRepository.GetVicePresidentAsync(ct) is { } existingVp)
        return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
            DistrictEmployeeError.VicePresidentExists(existingVp.Id));

    string? accountEmail = null;
    string? accountTempPassword = null;
    string? userId = null;

    if (request.MemberId is { } memberId)
    {
        var memberUser = await FindMemberAccountAsync(memberId, ct);
        if (memberUser is null)
        {
            var memberEntity = await dbContext.Members.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == memberId, ct);

            accountEmail = await GenerateUniqueDistrictStaffEmailAsync(districtCode, ct);
            accountTempPassword = GenerateTempPassword();

            memberUser = new User
            {
                UserName = accountEmail,
                Email = accountEmail,
                FirstName = memberEntity?.FirstName ?? string.Empty,
                FatherName = memberEntity?.FatherName ?? string.Empty,
                GrandfatherName = memberEntity?.GrandfatherName ?? string.Empty
            };

            var result = await userManager.CreateAsync(memberUser, accountTempPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                    DistrictEmployeeError.AccountCreationFailed(errors));
            }

            await EnsureRoleAsync("Member");
            await userManager.AddToRoleAsync(memberUser, "Member");
        }
        else
        {
            accountEmail = memberUser.Email;
        }

        userId = memberUser.Id;
    }
    else
    {
        accountEmail = await GenerateUniqueDistrictStaffEmailAsync(districtCode, ct);
        accountTempPassword = GenerateTempPassword();

        var user = new User
        {
            UserName = accountEmail,
            Email = accountEmail,
            FirstName = request.FirstName!.Trim(),
            FatherName = request.FatherName!.Trim(),
            GrandfatherName = request.GrandfatherName!.Trim()
        };

        var result = await userManager.CreateAsync(user, accountTempPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.AccountCreationFailed(errors));
        }

        userId = user.Id;
    }
    // ↑ else block ends here — Employee creation is now unconditional below

    var employee = new Employee
    {
        DistrictId = districtId,
        ChurchId = null,
        UserId = userId,
        Position = request.Position!.Trim(),
        EmploymentType = employmentType,
        MinisterTitle = request.MinisterTitle,
        MemberId = request.MemberId,
        FirstName = request.MemberId is null ? request.FirstName?.Trim() : null,
        FatherName = request.MemberId is null ? request.FatherName?.Trim() : null,
        GrandfatherName = request.MemberId is null ? request.GrandfatherName?.Trim() : null,
        Salary = request.Salary,
        SalaryPaidBy = SalaryPaidBy.District,
        HireDate = request.HireDate,
        Status = EmployeeStatus.Active,
        IsDistrictPresident = request.IsDistrictPresident,
        IsVicePresident = request.IsVicePresident
    };

    var created = await employeeRepository.AddAsync(employee, ct);

    if (request.MemberId is { } linkedMemberId)
        await employeeRepository.MarkChurchEmployeesPaidByDistrictAsync(linkedMemberId, ct);

    logger.LogInformation("Created office employee {EmployeeId} ({Position}, {EmploymentType})",
        created.Id, created.Position, created.EmploymentType);

    var dto = ToDto(created) with { AccountEmail = accountEmail, AccountTempPassword = accountTempPassword };

    return Result<EmployeeResponseDto, DistrictEmployeeError>.Success(dto);
}

    private async Task<User?> FindMemberAccountAsync(long memberId, CancellationToken ct)
    {
        var linkedUserId = await dbContext.Employees.AsNoTracking()
            .Where(e => e.MemberId == memberId && e.UserId != null && !e.IsDeleted)
            .Select(e => e.UserId)
            .FirstOrDefaultAsync(ct);

        return linkedUserId is null ? null : await userManager.FindByIdAsync(linkedUserId);
    }

    private async Task EnsureRoleAsync(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task<string> GenerateUniqueDistrictStaffEmailAsync(string districtCode, CancellationToken ct)
    {
        var baseEmail = $"staff.{districtCode.ToLowerInvariant()}@icms.et";

        if (await userManager.FindByEmailAsync(baseEmail) is null)
        {
            return baseEmail;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"staff.{districtCode.ToLowerInvariant()}{suffix}@icms.et";
            if (await userManager.FindByEmailAsync(candidate) is null)
            {
                return candidate;
            }
        }
    }

    private static string GenerateTempPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*";
        Func<int, int> random = RandomNumberGenerator.GetInt32;

        var chars = new List<char>
        {
            upper[random(upper.Length)],
            lower[random(lower.Length)],
            digits[random(digits.Length)],
            symbols[random(symbols.Length)]
        };

        var pool = upper + lower + digits + symbols;
        for (var i = 4; i < 16; i++)
        {
            chars.Add(pool[random(pool.Length)]);
        }

        return new string(chars.OrderBy(_ => random(int.MaxValue)).ToArray());
    }

    public async Task<Result<EmployeeResponseDto, DistrictEmployeeError>> GetEmployeeAsync(
        long districtId, long employeeId, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        var employee = await employeeRepository.GetEmployeeAsync(employeeId, ct);

        return employee is null
            ? Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.NotFound(employeeId))
            : Result<EmployeeResponseDto, DistrictEmployeeError>.Success(employee.ToDto());
    }

    public async Task<Result<EmployeeResponseDto, DistrictEmployeeError>> UpdateEmployeeAsync(
        long districtId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        await updateValidator.ValidateAndThrowAsync(request, ct);

        var employee = await employeeRepository.GetTrackedEmployeeAsync(employeeId, ct);
        if (employee is null)
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.NotFound(employeeId));

        if (request.IsDistrictPresident && !employee.IsDistrictPresident
            && await employeeRepository.GetPresidentAsync(ct) is { } existing)
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.PresidentExists(existing.Id));

        if (request.IsVicePresident && !employee.IsVicePresident
            && await employeeRepository.GetVicePresidentAsync(ct) is { } existingVp)
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.VicePresidentExists(existingVp.Id));

        if (request.IsDistrictPresident && !employee.IsDistrictPresident
            && (employee.MemberId is null || employee.EmploymentType != EmploymentType.FulltimeMinister))
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.PresidentMustBeMinister());

        if (request.IsVicePresident && !employee.IsVicePresident
            && (employee.MemberId is null || employee.EmploymentType != EmploymentType.FulltimeMinister))
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.VicePresidentMustBeMinister());

        if (request.IsDistrictPresident && !employee.IsDistrictPresident && employee.MemberId is { } memberId)
        {
            var member = await employeeRepository.GetMemberInDistrictAsync(districtId, memberId, ct);
            if (member?.ChurchType != ChurchType.Local)
                return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                    DistrictEmployeeError.MemberNotLocalChurch(memberId));
        }

        if (request.IsVicePresident && !employee.IsVicePresident && employee.MemberId is { } memberId2)
        {
            var member = await employeeRepository.GetMemberInDistrictAsync(districtId, memberId2, ct);
            if (member?.ChurchType != ChurchType.Local)
                return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                    DistrictEmployeeError.MemberNotLocalChurch(memberId2));
        }

        employee.Position = request.Position!.Trim();
        employee.MinisterTitle = request.MinisterTitle;
        employee.Salary = request.Salary;
        employee.HireDate = request.HireDate;
        employee.Status = request.Status ?? employee.Status;
        employee.IsDistrictPresident = request.IsDistrictPresident;
        employee.IsVicePresident = request.IsVicePresident;

        var updated = await employeeRepository.UpdateAsync(employee, ct);

        logger.LogInformation("Updated office employee {EmployeeId} ({Position})",
            updated.Id, updated.Position);

        return Result<EmployeeResponseDto, DistrictEmployeeError>.Success(ToDto(updated));
    }

    public async Task<Result<PagedResponse<DistrictMinisterDto>, DistrictEmployeeError>> GetMinistersAsync(
        long districtId, string? search, int page, int pageSize, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<PagedResponse<DistrictMinisterDto>, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        var placements = await employeeRepository.GetMinisterPlacementsAsync(districtId, search, ct);

        var people = placements
            .GroupBy(p => p.MemberId)
            .Select(g => new DistrictMinisterDto(
                g.Key,
                g.First().FullName,
                g.First().EfgbcId,
                g.First().HomeChurchName,
                g.FirstOrDefault(p => p.ChurchId is null)?.MinisterTitle ?? g.First().MinisterTitle,
                g.Any(p => p.SalaryPaidBy == SalaryPaidBy.District),
                g.Select(p => new MinisterPlacementDto(
                    p.Scope, p.ChurchId, p.Position, p.IsPresident, p.IsVicePresident, p.SalaryPaidBy))
                    .ToList()))
            .OrderBy(m => m.FullName)
            .ToList();

        var total = people.Count;
        var items = people.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResponse<DistrictMinisterDto>, DistrictEmployeeError>.Success(
            new PagedResponse<DistrictMinisterDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    public async Task<Result<EmployeeResponseDto, DistrictEmployeeError>> DeleteEmployeeAsync(
        long districtId, long employeeId, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        var employee = await employeeRepository.GetTrackedEmployeeAsync(employeeId, ct);
        if (employee is null)
            return Result<EmployeeResponseDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.NotFound(employeeId));

        employee.IsDeleted = true;

        var memberId = employee.MemberId;

        var deleted = await employeeRepository.UpdateAsync(employee, ct);

        if (memberId is { } leavingMemberId
            && !await employeeRepository.MemberHasActiveDistrictEmploymentAsync(leavingMemberId, ct))
            await employeeRepository.RevertChurchEmployeesToChurchPaidAsync(leavingMemberId, ct);

        logger.LogInformation("Deleted office employee {EmployeeId} ({Position})",
            deleted.Id, deleted.Position);

        return Result<EmployeeResponseDto, DistrictEmployeeError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<EmployeeResponseDto>, DistrictEmployeeError>> GetEmployeesAsync(
        long districtId, PagedRequest paging, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<PagedResponse<EmployeeResponseDto>, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        var page = paging.SafePage;

        var employees = await employeeRepository.GetEmployeesAsync(
            paging.Search, page, paging.PageSize, ct);
        var totalCount = await employeeRepository.CountEmployeesAsync(paging.Search, ct);

        return Result<PagedResponse<EmployeeResponseDto>, DistrictEmployeeError>.Success(
            new PagedResponse<EmployeeResponseDto>
            {
                Items = employees.Select(e => e.ToDto()).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<OfficeExecutivesDto, DistrictEmployeeError>> GetExecutivesAsync(
        long districtId, CancellationToken ct)
    {
        if (!await employeeRepository.DistrictExistsAsync(districtId, ct))
            return Result<OfficeExecutivesDto, DistrictEmployeeError>.Failure(
                DistrictEmployeeError.DistrictNotFound(districtId));

        var president = await employeeRepository.GetPresidentAsync(ct);
        var vicePresident = await employeeRepository.GetVicePresidentAsync(ct);

        return Result<OfficeExecutivesDto, DistrictEmployeeError>.Success(
            new OfficeExecutivesDto(president?.ToDto(), vicePresident?.ToDto()));
    }

    private static EmployeeResponseDto ToDto(Employee e)
    {
        var memberName = e.Member is null ? null : MemberNames.Full(e.Member);
        if (memberName is null)
        {
            var stored = string.Join(" ",
                new[] { e.FirstName, e.FatherName, e.GrandfatherName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            memberName = string.IsNullOrWhiteSpace(stored) ? null : stored;
        }

        return new(e.Id, e.Position, e.EmploymentType, e.MinisterTitle, e.MemberId,
            memberName,
            e.Member?.EfgbcId, e.Member?.ChurchId, e.Member?.Church?.Name,
            e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate,
            e.Status, e.IsDistrictPresident, e.IsVicePresident, null, null);
    }
}