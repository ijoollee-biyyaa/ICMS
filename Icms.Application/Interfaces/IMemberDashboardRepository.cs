using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface IMemberDashboardRepository
{
    Task<Member?> GetMemberAsync(long memberId, CancellationToken ct);

    Task<List<TeamMembershipRow>> GetMembershipsAsync(long memberId, CancellationToken ct);

    Task<List<TeamMeetingRow>> GetTeamMeetingsAsync(long memberId, CancellationToken ct);

    Task<List<MemberAttendanceRow>> GetAttendanceRowsAsync(long memberId, CancellationToken ct);

    Task<List<MemberPaymentRow>> GetPaymentRowsAsync(long memberId, CancellationToken ct);

    Task<List<MemberAttendanceHistoryRow>> GetAttendanceHistoryAsync(long memberId, CancellationToken ct);

    Task<List<MemberPaymentHistoryRow>> GetPaymentHistoryAsync(long memberId, CancellationToken ct);

    Task<List<EmployeeServiceRow>> GetEmployeeRowsAsync(long memberId, CancellationToken ct);

    Task<List<DepartmentAssignmentRow>> GetDepartmentRowsAsync(long memberId, CancellationToken ct);
}

public record TeamMembershipRow(long TeamId, string TeamName, TeamMemberRole Role, long ChurchId);

public record TeamMeetingRow(long TeamId, DateOnly AttendanceDate);

public record MemberAttendanceRow(long TeamId, DateOnly AttendanceDate, TeamAttendanceStatus Status);

public record MemberPaymentRow(long TeamId, DateOnly Month, decimal Amount);

public record MemberAttendanceHistoryRow(long TeamId, string TeamName, DateOnly AttendanceDate, TeamAttendanceStatus Status, string? Reason);

public record MemberPaymentHistoryRow(long Id, long TeamId, string TeamName, DateOnly Month, decimal Amount, DateTimeOffset PaidAt);

public record EmployeeServiceRow(
    long Id, string Position, EmploymentType EmploymentType,
    bool IsPresident, bool IsVicePresident, long? ChurchId, string? ChurchName);

public record DepartmentAssignmentRow(
    long EmployeeId, long DepartmentId, string DepartmentName, DepartmentEmployeeRole Role);