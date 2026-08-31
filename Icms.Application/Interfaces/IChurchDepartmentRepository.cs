using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface IChurchDepartmentRepository
{
    Task<bool> ChurchExistsAsync(long churchId, CancellationToken ct);
    Task<DepartmentRow?> GetDepartmentAsync(long churchId, long departmentId, CancellationToken ct);
    Task<Department?> GetTrackedDepartmentAsync(long churchId, long departmentId, CancellationToken ct);
    Task<List<DepartmentRow>> GetDepartmentsAsync(long churchId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountDepartmentsAsync(long churchId, string? search, CancellationToken ct);
    Task<Department> AddAsync(Department department, CancellationToken ct);
    Task<Department> UpdateAsync(Department department, CancellationToken ct);
    Task<bool> HeadIsChurchEmployeeAsync(long employeeId, long churchId, CancellationToken ct);
    Task<bool> EmployeeInChurchAsync(long employeeId, long churchId, CancellationToken ct);
    Task<List<DepartmentEmployeeRow>> GetEmployeesAsync(long departmentId, CancellationToken ct);
    Task<DepartmentEmployee?> GetAssignmentAsync(long departmentId, long employeeId, CancellationToken ct);
    Task<DepartmentEmployee?> GetAssignmentByIdAsync(long departmentId, long id, CancellationToken ct);
    Task<DepartmentEmployee> AssignAsync(DepartmentEmployee assignment, CancellationToken ct);
    Task RemoveAssignmentAsync(DepartmentEmployee assignment, CancellationToken ct);
}