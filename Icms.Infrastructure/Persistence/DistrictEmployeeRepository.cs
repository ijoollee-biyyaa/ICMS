using Microsoft.EntityFrameworkCore;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class DistrictEmployeeRepository(IcmsDbContext dbContext) : IDistrictEmployeeRepository
{
    public Task<bool> DistrictExistsAsync(long districtId, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .AnyAsync(d => d.Id == districtId && !d.IsDeleted, ct);

    public Task<Employee?> GetTrackedEmployeeAsync(long employeeId, CancellationToken ct) =>
        dbContext.Employees
            .SingleOrDefaultAsync(e => e.Id == employeeId && e.ChurchId == null, ct);

    public Task<EmployeeRow?> GetEmployeeAsync(long employeeId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.Id == employeeId && e.ChurchId == null)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .SingleOrDefaultAsync(ct);

    public Task<List<EmployeeRow>> GetEmployeesAsync(
        string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == null
                && (search == null
                    || EF.Functions.ILike(e.Position, $"%{search}%")
                    || (e.Member != null && EF.Functions.ILike(e.Member.FirstName, $"%{search}%"))
                    || (e.Member == null && EF.Functions.ILike(e.FirstName, $"%{search}%"))))
            .OrderBy(e => e.Position)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountEmployeesAsync(string? search, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .CountAsync(e => e.ChurchId == null
                && (search == null
                    || EF.Functions.ILike(e.Position, $"%{search}%")
                    || (e.Member != null && EF.Functions.ILike(e.Member.FirstName, $"%{search}%"))
                    || (e.Member == null && EF.Functions.ILike(e.FirstName, $"%{search}%"))), ct);

    public Task<int> MarkChurchEmployeesPaidByDistrictAsync(long memberId, CancellationToken ct) =>
        dbContext.Employees
            .Where(e => e.ChurchId != null && e.MemberId == memberId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.SalaryPaidBy, SalaryPaidBy.District), ct);

    public Task<bool> MemberHasActiveDistrictEmploymentAsync(long memberId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .AnyAsync(e => e.ChurchId == null && e.MemberId == memberId, ct);

    public Task<int> RevertChurchEmployeesToChurchPaidAsync(long memberId, CancellationToken ct) =>
        dbContext.Employees
            .Where(e => e.ChurchId != null && e.MemberId == memberId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.SalaryPaidBy, SalaryPaidBy.Church), ct);

    public Task<int> CountMinistersAsync(long districtId, string? search, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.EmploymentType == EmploymentType.FulltimeMinister
                && e.MemberId != null
                && (e.ChurchId == null || e.Church.DistrictId == districtId)
                && (search == null || EF.Functions.ILike(
                    e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName,
                    $"%{search}%")))
            .Select(e => e.MemberId)
            .Distinct()
            .CountAsync(ct);

    public Task<List<MinisterPlacementRow>> GetMinisterPlacementsAsync(
        long districtId, string? search, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.EmploymentType == EmploymentType.FulltimeMinister
                && e.MemberId != null
                && (e.ChurchId == null || e.Church.DistrictId == districtId)
                && (search == null || EF.Functions.ILike(
                    e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName,
                    $"%{search}%")))
            .OrderBy(e => e.Member.FirstName)
            .Select(e => new MinisterPlacementRow(
                (long)e.MemberId,
                e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName,
                e.Member.EfgbcId,
                e.Member.Church.Name,
                e.ChurchId == null ? "District Office" : e.Church.Name,
                e.ChurchId,
                e.Position,
                e.MinisterTitle,
                e.IsDistrictPresident,
                e.IsVicePresident,
                e.SalaryPaidBy))
            .ToListAsync(ct);

    public Task<EmployeeRow?> GetPresidentAsync(CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == null && e.IsDistrictPresident)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .SingleOrDefaultAsync(ct);

    public Task<EmployeeRow?> GetVicePresidentAsync(CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == null && e.IsVicePresident)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .SingleOrDefaultAsync(ct);

    public Task<EmployeeRow?> GetActivePresidentExcludingAsync(long excludeId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == null && e.IsDistrictPresident && e.Id != excludeId)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .SingleOrDefaultAsync(ct);

    public Task<EmployeeRow?> GetActiveVicePresidentExcludingAsync(long excludeId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == null && e.IsVicePresident && e.Id != excludeId)
            .Select(e => new EmployeeRow(e.Id, e.Position, e.EmploymentType, e.MinisterTitle,
                e.MemberId,                 e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName
    : (e.FirstName != null ? (e.FirstName + " " + e.FatherName + " " + e.GrandfatherName).Trim() : null),
                e.Member != null ? e.Member.EfgbcId : null,
                e.Member != null ? (long?)e.Member.ChurchId : null,
                e.Member != null ? e.Member.Church.Name : null,
                e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident))
            .SingleOrDefaultAsync(ct);

    public Task<MemberRow?> GetMemberInDistrictAsync(long districtId, long memberId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .Where(m => m.Id == memberId && m.Church.DistrictId == districtId && !m.IsDeleted)
            .Select(m => new MemberRow(m.Id, m.FirstName, m.EfgbcId, m.Church.Id, m.Church.Name, m.Church.Type))
            .SingleOrDefaultAsync(ct);

    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct)
    {
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(employee).Reference(e => e.Member).LoadAsync(ct);
        if (employee.Member is not null)
            await dbContext.Entry(employee.Member).Reference(m => m.Church).LoadAsync(ct);
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken ct)
    {
        dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(employee).Reference(e => e.Member).LoadAsync(ct);
        if (employee.Member is not null)
            await dbContext.Entry(employee.Member).Reference(m => m.Church).LoadAsync(ct);
        return employee;
    }
}