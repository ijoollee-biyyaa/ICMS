using Microsoft.EntityFrameworkCore;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class ChurchEmployeeRepository(IcmsDbContext dbContext) : IChurchEmployeeRepository
{
    public Task<bool> ChurchExistsAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .AnyAsync(c => c.Id == churchId && !c.IsDeleted, ct);

    public Task<MemberRow?> GetMemberInChurchAsync(long churchId, long memberId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .Where(m => m.Id == memberId && m.ChurchId == churchId && !m.IsDeleted)
            .Select(m => new MemberRow(m.Id, m.FirstName + " " + m.FatherName + " " + m.GrandfatherName, m.EfgbcId,
                m.Church.Id, m.Church.Name, m.Church.Type))
            .SingleOrDefaultAsync(ct);

    public async Task<EmployeeRow?> GetEmployeeAsync(long churchId, long employeeId, CancellationToken ct)
    {
        var row = await dbContext.Employees.AsNoTracking()
            .Where(e => e.Id == employeeId && e.ChurchId == churchId)
            .Select(e => new
            {
                e.Id, e.Position, e.EmploymentType, e.MinisterTitle, e.MemberId,
                e.UserId, e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident,
                e.FirstName, e.FatherName, e.GrandfatherName,
                MemberName = e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName : null,
                MemberEfgbcId = e.Member != null ? e.Member.EfgbcId : null,
                MemberChurchId = e.Member != null ? (long?)e.Member.ChurchId : null,
                MemberChurchName = e.Member != null ? e.Member.Church.Name : null
            })
            .SingleOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        var accountNames = await LoadAccountNamesAsync(
            row.MemberName is null ? new[] { row.UserId! } : Array.Empty<string>(), ct);

        return ToEmployeeRow(row, accountNames);
    }

    public Task<Employee?> GetTrackedEmployeeAsync(long churchId, long employeeId, CancellationToken ct) =>
        dbContext.Employees
            .SingleOrDefaultAsync(e => e.Id == employeeId && e.ChurchId == churchId, ct);

    public async Task<List<EmployeeRow>> GetEmployeesAsync(
        long churchId, string? search, int page, int pageSize, CancellationToken ct)
    {
        var rows = await dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId == churchId
                && (search == null
                    || EF.Functions.ILike(e.Position, $"%{search}%")
                    || (e.Member != null && EF.Functions.ILike(e.Member.FirstName, $"%{search}%"))
                    || (e.Member == null && EF.Functions.ILike(e.FirstName, $"%{search}%"))))
            .OrderBy(e => e.Position)
            .Select(e => new
            {
                e.Id, e.Position, e.EmploymentType, e.MinisterTitle, e.MemberId,
                e.UserId, e.ChurchId, e.Salary, e.SalaryPaidBy, e.HireDate, e.Status,
                e.IsDistrictPresident, e.IsVicePresident,
                e.FirstName, e.FatherName, e.GrandfatherName,
                MemberName = e.Member != null ? e.Member.FirstName + " " + e.Member.FatherName + " " + e.Member.GrandfatherName : null,
                MemberEfgbcId = e.Member != null ? e.Member.EfgbcId : null,
                MemberChurchId = e.Member != null ? (long?)e.Member.ChurchId : null,
                MemberChurchName = e.Member != null ? e.Member.Church.Name : null
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var accountNames = await LoadAccountNamesAsync(
            rows.Where(r => r.MemberName is null && r.UserId is not null).Select(r => r.UserId!).Distinct().ToArray(),
            ct);

        return rows.Select(r => ToEmployeeRow(r, accountNames)).ToList();
    }

    public Task<int> CountEmployeesAsync(long churchId, string? search, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .CountAsync(e => e.ChurchId == churchId
                && (search == null
                    || EF.Functions.ILike(e.Position, $"%{search}%")
                    || (e.Member != null && EF.Functions.ILike(e.Member.FirstName, $"%{search}%"))
                    || (e.Member == null && EF.Functions.ILike(e.FirstName, $"%{search}%"))), ct);

    private async Task<Dictionary<string, string?>> LoadAccountNamesAsync(
        IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds.ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<string, string?>();
        }

        return await dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(
                u => u.Id,
                u => $"{u.FirstName} {u.FatherName} {u.GrandfatherName}".Trim(),
                ct);
    }

    private static EmployeeRow ToEmployeeRow(dynamic row,
        Dictionary<string, string?> accountNames)
    {
        string? memberName = row.MemberName;
        if (memberName is null && row.UserId is not null)
        {
            accountNames.TryGetValue(row.UserId, out memberName);
        }
        if (memberName is null)
        {
            var stored = string.Join(" ",
                new[] { row.FirstName, row.FatherName, row.GrandfatherName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            memberName = string.IsNullOrWhiteSpace(stored) ? null : stored;
        }

        return new EmployeeRow(row.Id, row.Position, row.EmploymentType, row.MinisterTitle,
            row.MemberId, memberName, row.MemberEfgbcId,
            row.MemberChurchId, row.MemberChurchName,
            row.ChurchId, row.Salary, row.SalaryPaidBy, row.HireDate, row.Status,
            row.IsDistrictPresident, row.IsVicePresident);
    }

    public Task<bool> EmployeeInChurchAsync(long employeeId, long churchId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .AnyAsync(e => e.Id == employeeId && e.ChurchId == churchId, ct);

    public Task<bool> MemberIsDistrictEmployeeAsync(long memberId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .AnyAsync(e => e.ChurchId == null && e.MemberId == memberId, ct);

    /// <summary>
    /// Finds the member's existing login account: first one linked directly (AspNetUsers.MemberId),
    /// then any account already linked to one of the member's employee records.
    /// </summary>
    public async Task<string?> FindMemberAccountUserIdAsync(long memberId, CancellationToken ct)
    {
        var direct = await dbContext.Users.AsNoTracking()
            .Where(u => u.MemberId == memberId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (direct is not null)
        {
            return direct;
        }

        return await dbContext.Employees.AsNoTracking()
            .Where(e => e.MemberId == memberId && e.UserId != null && !e.IsDeleted)
            .Select(e => e.UserId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct)
    {
        dbContext.Employees.Add(employee);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Member {employee.MemberId} is already an employee of church {employee.ChurchId}.", ex);
        }

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

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        for (var e = ex.InnerException; e is not null; e = e.InnerException)
        {
            if (e is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
                return true;
        }

        return false;
    }
}