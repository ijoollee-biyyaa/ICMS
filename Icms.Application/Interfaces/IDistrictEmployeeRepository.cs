using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface IDistrictEmployeeRepository
{
    Task<EmployeeRow?> GetEmployeeAsync(long employeeId, CancellationToken ct);

    Task<int> MarkChurchEmployeesPaidByDistrictAsync(long memberId, CancellationToken ct);

    Task<bool> MemberHasActiveDistrictEmploymentAsync(long memberId, CancellationToken ct);

    Task<int> RevertChurchEmployeesToChurchPaidAsync(long memberId, CancellationToken ct);

    Task<int> CountMinistersAsync(long districtId, string? search, CancellationToken ct);

    Task<List<MinisterPlacementRow>> GetMinisterPlacementsAsync(
        long districtId, string? search, CancellationToken ct);
    Task<Employee?> GetTrackedEmployeeAsync(long employeeId, CancellationToken ct);
    Task<bool> DistrictExistsAsync(long districtId, CancellationToken ct);
    Task<List<EmployeeRow>> GetEmployeesAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountEmployeesAsync(string? search, CancellationToken ct);
    Task<EmployeeRow?> GetPresidentAsync(CancellationToken ct);
    Task<EmployeeRow?> GetVicePresidentAsync(CancellationToken ct);
    Task<EmployeeRow?> GetActivePresidentExcludingAsync(long excludeId, CancellationToken ct);
    Task<EmployeeRow?> GetActiveVicePresidentExcludingAsync(long excludeId, CancellationToken ct);
    Task<MemberRow?> GetMemberInDistrictAsync(long districtId, long memberId, CancellationToken ct);
    Task<Employee> AddAsync(Employee employee, CancellationToken ct);
    Task<Employee> UpdateAsync(Employee employee, CancellationToken ct);
}

public record EmployeeRow(long Id, string Position, EmploymentType EmploymentType,
    MinisterTitle? MinisterTitle, long? MemberId, string? MemberName, string? MemberEfgbcId,
    long? MemberChurchId, string? MemberChurchName,
    long? ChurchId, decimal? Salary, SalaryPaidBy SalaryPaidBy,
    DateOnly? HireDate, EmployeeStatus Status,
    bool IsDistrictPresident, bool IsVicePresident)
{
    public EmployeeResponseDto ToDto() =>
        new(Id, Position, EmploymentType, MinisterTitle, MemberId, MemberName, MemberEfgbcId,
            MemberChurchId, MemberChurchName,
            ChurchId, Salary, SalaryPaidBy, HireDate, Status, IsDistrictPresident, IsVicePresident,
            null, null);
}

public record MemberRow(long Id, string MemberName, string MemberEfgbcId,
    long ChurchId, string ChurchName, ChurchType ChurchType);

public record MinisterPlacementRow(
    long MemberId, string FullName, string EfgbcId, string? HomeChurchName,
    string Scope, long? ChurchId, string Position, MinisterTitle? MinisterTitle,
    bool IsPresident, bool IsVicePresident, SalaryPaidBy SalaryPaidBy);