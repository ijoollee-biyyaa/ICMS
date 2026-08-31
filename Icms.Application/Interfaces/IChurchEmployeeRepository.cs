using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface IChurchEmployeeRepository
{
    Task<bool> ChurchExistsAsync(long churchId, CancellationToken ct);
    Task<MemberRow?> GetMemberInChurchAsync(long churchId, long memberId, CancellationToken ct);
    Task<EmployeeRow?> GetEmployeeAsync(long churchId, long employeeId, CancellationToken ct);
    Task<Employee?> GetTrackedEmployeeAsync(long churchId, long employeeId, CancellationToken ct);
    Task<List<EmployeeRow>> GetEmployeesAsync(long churchId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountEmployeesAsync(long churchId, string? search, CancellationToken ct);
    Task<bool> EmployeeInChurchAsync(long employeeId, long churchId, CancellationToken ct);
    Task<bool> MemberIsDistrictEmployeeAsync(long memberId, CancellationToken ct);
    Task<string?> FindMemberAccountUserIdAsync(long memberId, CancellationToken ct);
    Task<Employee> AddAsync(Employee employee, CancellationToken ct);
    Task<Employee> UpdateAsync(Employee employee, CancellationToken ct);
}