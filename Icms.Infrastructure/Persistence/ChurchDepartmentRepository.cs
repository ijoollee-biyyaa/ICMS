using Microsoft.EntityFrameworkCore;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class ChurchDepartmentRepository(IcmsDbContext dbContext) : IChurchDepartmentRepository
{
    public Task<bool> ChurchExistsAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .AnyAsync(c => c.Id == churchId && !c.IsDeleted, ct);

    public Task<DepartmentRow?> GetDepartmentAsync(long churchId, long departmentId, CancellationToken ct) =>
        dbContext.Departments.AsNoTracking()
            .Where(d => d.Id == departmentId && d.ChurchId == churchId)
            .Select(d => new DepartmentRow(d.Id, d.Name, d.Type, d.HeadEmployeeId,
                d.HeadEmployee != null ? d.HeadEmployee.Position : null,
                d.Employees.Count))
            .SingleOrDefaultAsync(ct);

    public Task<Department?> GetTrackedDepartmentAsync(long churchId, long departmentId, CancellationToken ct) =>
        dbContext.Departments
            .SingleOrDefaultAsync(d => d.Id == departmentId && d.ChurchId == churchId, ct);

    public Task<List<DepartmentRow>> GetDepartmentsAsync(
        long churchId, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.Departments.AsNoTracking()
            .Where(d => d.ChurchId == churchId
                && (search == null || EF.Functions.ILike(d.Name, $"%{search}%")))
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentRow(d.Id, d.Name, d.Type, d.HeadEmployeeId,
                d.HeadEmployee != null ? d.HeadEmployee.Position : null,
                d.Employees.Count))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountDepartmentsAsync(long churchId, string? search, CancellationToken ct) =>
        dbContext.Departments.AsNoTracking()
            .CountAsync(d => d.ChurchId == churchId
                && (search == null || EF.Functions.ILike(d.Name, $"%{search}%")), ct);

    public async Task<Department> AddAsync(Department department, CancellationToken ct)
    {
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync(ct);
        return department;
    }

    public async Task<Department> UpdateAsync(Department department, CancellationToken ct)
    {
        dbContext.Departments.Update(department);
        await dbContext.SaveChangesAsync(ct);
        return department;
    }

    public Task<bool> HeadIsChurchEmployeeAsync(long employeeId, long churchId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .AnyAsync(e => e.Id == employeeId && e.ChurchId == churchId, ct);

    public Task<bool> EmployeeInChurchAsync(long employeeId, long churchId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .AnyAsync(e => e.Id == employeeId && e.ChurchId == churchId, ct);

    public Task<List<DepartmentEmployeeRow>> GetEmployeesAsync(long departmentId, CancellationToken ct) =>
        dbContext.DepartmentEmployees.AsNoTracking()
            .Where(de => de.DepartmentId == departmentId)
            .OrderBy(de => de.Employee.Member!.FirstName)
            .Select(de => new DepartmentEmployeeRow(de.Id, de.EmployeeId,
                de.Employee.Member != null
                    ? de.Employee.Member.FirstName + " " + de.Employee.Member.FatherName + " " + de.Employee.Member.GrandfatherName
                    : null,
                de.Employee.Position, de.Role))
            .ToListAsync(ct);

    public Task<DepartmentEmployee?> GetAssignmentAsync(long departmentId, long employeeId, CancellationToken ct) =>
        dbContext.DepartmentEmployees
            .SingleOrDefaultAsync(de => de.DepartmentId == departmentId && de.EmployeeId == employeeId, ct);

    public Task<DepartmentEmployee?> GetAssignmentByIdAsync(long departmentId, long id, CancellationToken ct) =>
        dbContext.DepartmentEmployees
            .Include(de => de.Employee)
            .SingleOrDefaultAsync(de => de.DepartmentId == departmentId && de.Id == id, ct);

    public async Task<DepartmentEmployee> AssignAsync(DepartmentEmployee assignment, CancellationToken ct)
    {
        dbContext.DepartmentEmployees.Add(assignment);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Employee {assignment.EmployeeId} is already assigned to department {assignment.DepartmentId}.", ex);
        }

        await dbContext.Entry(assignment).Reference(de => de.Employee).LoadAsync(ct);
        await dbContext.Entry(assignment.Employee).Reference(e => e.Member).LoadAsync(ct);
        return assignment;
    }

    public async Task RemoveAssignmentAsync(DepartmentEmployee assignment, CancellationToken ct)
    {
        dbContext.DepartmentEmployees.Remove(assignment);
        await dbContext.SaveChangesAsync(ct);
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