using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateDepartmentRequest(
    string? Name,
    DepartmentType? Type,
    long? HeadEmployeeId);

public record UpdateDepartmentRequest(
    string? Name,
    DepartmentType? Type,
    long? HeadEmployeeId);

public record DepartmentResponseDto(
    long Id,
    string Name,
    DepartmentType Type,
    long? HeadEmployeeId,
    string? HeadEmployeeName,
    int EmployeeCount);

public record AddDepartmentEmployeeRequest(
    long? EmployeeId,
    DepartmentEmployeeRole? Role);

public record DepartmentEmployeeDto(
    long Id,
    long EmployeeId,
    string? EmployeeName,
    string EmployeePosition,
    DepartmentEmployeeRole Role);

public record CreateEmployeeRequest(
    string? Position,
    EmploymentType? EmploymentType,
    MinisterTitle? MinisterTitle,
    long? MemberId,
    decimal? Salary,
    DateOnly? HireDate,
    bool IsDistrictPresident,
    bool IsVicePresident,
    string? FirstName,
    string? FatherName,
    string? GrandfatherName);

public record UpdateEmployeeRequest(
    string? Position,
    MinisterTitle? MinisterTitle,
    decimal? Salary,
    DateOnly? HireDate,
    EmployeeStatus? Status,
    bool IsDistrictPresident,
    bool IsVicePresident);

public record EmployeeResponseDto(
    long Id,
    string Position,
    EmploymentType EmploymentType,
    MinisterTitle? MinisterTitle,
    long? MemberId,
    string? MemberName,
    string? MemberEfgbcId,
    long? MemberChurchId,
    string? MemberChurchName,
    long? ChurchId,
    decimal? Salary,
    SalaryPaidBy SalaryPaidBy,
    DateOnly? HireDate,
    EmployeeStatus Status,
    bool IsDistrictPresident,
    bool IsVicePresident,
    string? AccountEmail,
    string? AccountTempPassword);

public record OfficeExecutivesDto(
    EmployeeResponseDto? President,
    EmployeeResponseDto? VicePresident);

public record DistrictMinisterDto(
    long MemberId,
    string FullName,
    string EfgbcId,
    string? HomeChurchName,
    MinisterTitle? MinisterTitle,
    bool IsDistrictPaid,
    List<MinisterPlacementDto> Placements);

public record MinisterPlacementDto(
    string Scope,
    long? ChurchId,
    string Position,
    bool IsPresident,
    bool IsVicePresident,
    SalaryPaidBy SalaryPaidBy);