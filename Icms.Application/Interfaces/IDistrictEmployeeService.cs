using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IDistrictEmployeeService
{
    Task<Result<EmployeeResponseDto, DistrictEmployeeError>> CreateEmployeeAsync(
        long districtId, CreateEmployeeRequest request, CancellationToken ct);

    Task<Result<EmployeeResponseDto, DistrictEmployeeError>> GetEmployeeAsync(
        long districtId, long employeeId, CancellationToken ct);

    Task<Result<EmployeeResponseDto, DistrictEmployeeError>> UpdateEmployeeAsync(
        long districtId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct);

    Task<Result<EmployeeResponseDto, DistrictEmployeeError>> DeleteEmployeeAsync(
        long districtId, long employeeId, CancellationToken ct);

    Task<Result<PagedResponse<EmployeeResponseDto>, DistrictEmployeeError>> GetEmployeesAsync(
        long districtId, PagedRequest paging, CancellationToken ct);

    Task<Result<OfficeExecutivesDto, DistrictEmployeeError>> GetExecutivesAsync(
        long districtId, CancellationToken ct);

    Task<Result<PagedResponse<DistrictMinisterDto>, DistrictEmployeeError>> GetMinistersAsync(
        long districtId, string? search, int page, int pageSize, CancellationToken ct);
}