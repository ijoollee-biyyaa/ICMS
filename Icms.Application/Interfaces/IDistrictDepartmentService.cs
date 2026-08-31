using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IDistrictDepartmentService
{
    Task<Result<DepartmentResponseDto, DistrictDepartmentError>> CreateDepartmentAsync(
        long districtId, CreateDepartmentRequest request, CancellationToken ct);

    Task<Result<DepartmentResponseDto, DistrictDepartmentError>> GetDepartmentAsync(
        long districtId, long departmentId, CancellationToken ct);

    Task<Result<DepartmentResponseDto, DistrictDepartmentError>> UpdateDepartmentAsync(
        long districtId, long departmentId, UpdateDepartmentRequest request, CancellationToken ct);

    Task<Result<DepartmentResponseDto, DistrictDepartmentError>> DeleteDepartmentAsync(
        long districtId, long departmentId, CancellationToken ct);

    Task<Result<PagedResponse<DepartmentResponseDto>, DistrictDepartmentError>> GetDepartmentsAsync(
        long districtId, PagedRequest paging, CancellationToken ct);

    Task<Result<List<DepartmentEmployeeDto>, DistrictDepartmentError>> GetEmployeesAsync(
        long districtId, long departmentId, CancellationToken ct);

    Task<Result<DepartmentEmployeeDto, DistrictDepartmentError>> AssignEmployeeAsync(
        long districtId, long departmentId, AddDepartmentEmployeeRequest request, CancellationToken ct);

    Task<Result<DepartmentEmployeeDto, DistrictDepartmentError>> RemoveEmployeeAsync(
        long districtId, long departmentId, long departmentEmployeeId, CancellationToken ct);
}
