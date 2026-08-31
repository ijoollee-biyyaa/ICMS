using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IChurchDepartmentService
{
    Task<Result<DepartmentResponseDto, ChurchDepartmentError>> CreateDepartmentAsync(
        long churchId, CreateDepartmentRequest request, CancellationToken ct);

    Task<Result<DepartmentResponseDto, ChurchDepartmentError>> GetDepartmentAsync(
        long churchId, long departmentId, CancellationToken ct);

    Task<Result<DepartmentResponseDto, ChurchDepartmentError>> UpdateDepartmentAsync(
        long churchId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct);

    Task<Result<DepartmentResponseDto, ChurchDepartmentError>> DeleteDepartmentAsync(
        long churchId, long departmentId, CancellationToken ct);

    Task<Result<PagedResponse<DepartmentResponseDto>, ChurchDepartmentError>> GetDepartmentsAsync(
        long churchId, PagedRequest paging, CancellationToken ct);

    Task<Result<List<DepartmentEmployeeDto>, ChurchDepartmentError>> GetEmployeesAsync(
        long churchId, long departmentId, CancellationToken ct);

    Task<Result<DepartmentEmployeeDto, ChurchDepartmentError>> AssignEmployeeAsync(
        long churchId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct);

    Task<Result<DepartmentEmployeeDto, ChurchDepartmentError>> RemoveEmployeeAsync(
        long churchId, long departmentId, long departmentEmployeeId, CancellationToken ct);
}