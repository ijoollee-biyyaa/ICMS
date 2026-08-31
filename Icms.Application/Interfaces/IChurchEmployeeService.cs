using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IChurchEmployeeService
{
    Task<Result<EmployeeResponseDto, ChurchEmployeeError>> CreateEmployeeAsync(
        long churchId, CreateEmployeeRequest request, CancellationToken ct);

    Task<Result<EmployeeResponseDto, ChurchEmployeeError>> GetEmployeeAsync(
        long churchId, long employeeId, CancellationToken ct);

    Task<Result<EmployeeResponseDto, ChurchEmployeeError>> UpdateEmployeeAsync(
        long churchId, long employeeId, UpdateEmployeeRequest request, CancellationToken ct);

    Task<Result<EmployeeResponseDto, ChurchEmployeeError>> DeleteEmployeeAsync(
        long churchId, long employeeId, CancellationToken ct);

    Task<Result<PagedResponse<EmployeeResponseDto>, ChurchEmployeeError>> GetEmployeesAsync(
        long churchId, PagedRequest paging, CancellationToken ct);
}