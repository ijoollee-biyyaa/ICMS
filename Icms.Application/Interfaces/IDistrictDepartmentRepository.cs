using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface IDistrictDepartmentRepository
{
    Task<bool> DistrictExistsAsync(long districtId, CancellationToken ct);
    Task<DepartmentRow?> GetDepartmentAsync(long departmentId, CancellationToken ct);
    Task<Department?> GetTrackedDepartmentAsync(long departmentId, CancellationToken ct);
    Task<List<DepartmentRow>> GetDepartmentsAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountDepartmentsAsync(string? search, CancellationToken ct);
    Task<Department> AddAsync(Department department, CancellationToken ct);
    Task<Department> UpdateAsync(Department department, CancellationToken ct);
    Task<bool> HeadIsOfficeEmployeeAsync(long employeeId, CancellationToken ct);
    Task<bool> EmployeeIsOfficeEmployeeAsync(long employeeId, CancellationToken ct);
    Task<List<DepartmentEmployeeRow>> GetEmployeesAsync(long departmentId, CancellationToken ct);
    Task<DepartmentEmployee?> GetAssignmentAsync(long departmentId, long employeeId, CancellationToken ct);
    Task<DepartmentEmployee?> GetAssignmentByIdAsync(long departmentId, long id, CancellationToken ct);
    Task<DepartmentEmployee> AssignAsync(DepartmentEmployee assignment, CancellationToken ct);
    Task RemoveAssignmentAsync(DepartmentEmployee assignment, CancellationToken ct);
}

public record DepartmentRow(long Id, string Name, DepartmentType Type,
    long? HeadEmployeeId, string? HeadEmployeeName, int EmployeeCount);

public record DepartmentEmployeeRow(long Id, long EmployeeId, string? EmployeeName,
    string EmployeePosition, DepartmentEmployeeRole Role);
