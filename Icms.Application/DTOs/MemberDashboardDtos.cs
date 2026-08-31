using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record MemberDashboardDto(
    long MemberId,
    MemberProfileDto Profile,
    int TeamCount,
    List<MemberTeamCardDto> Teams,
    MemberServiceSummaryDto Service);

public record MemberProfileDto(
    string FullName,
    string EfgbcId,
    string ChurchName,
    DateOnly? DateOfBirth,
    int? Age,
    Gender Gender,
    string? Phone,
    string? Email,
    MemberStatus Status,
    DateOnly? JoinedAt);

public record MemberTeamCardDto(
    long TeamId,
    string TeamName,
    string Role,
    MemberAttendanceSummaryDto Attendance,
    MemberPaymentSummaryDto Payments);

public record MemberAttendanceSummaryDto(
    int Meetings,
    int Present,
    int Late,
    int Absent,
    int Attended,
    double? AttendanceRate,
    DateOnly? LastAttendanceDate);

public record MemberPaymentSummaryDto(
    int PaymentCount,
    decimal TotalAmount,
    DateOnly? LastPaidMonth);

public record MemberServiceSummaryDto(
    int MeetingCount,
    int AttendedCount,
    double? AttendanceRate,
    decimal TotalPayments,
    List<MemberServiceDto> Services);

public record MemberServiceDto(
    long EmployeeId,
    string Scope,
    long? ChurchId,
    string? Position,
    EmploymentType? EmploymentType,
    bool IsPresident,
    bool IsVicePresident,
    List<MemberDepartmentAssignmentDto> Departments);

public record MemberDepartmentAssignmentDto(
    long DepartmentId,
    string DepartmentName,
    DepartmentEmployeeRole Role);