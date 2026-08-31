using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Services;

public class MemberDashboardService(IMemberDashboardRepository dashboardRepository)
    : IMemberDashboardService
{
    public async Task<Result<MemberDashboardDto, MemberDashboardError>> GetDashboardAsync(
        long memberId, CancellationToken ct)
    {
        var member = await dashboardRepository.GetMemberAsync(memberId, ct);
        if (member is null)
            return Result<MemberDashboardDto, MemberDashboardError>.Failure(
                MemberDashboardError.MemberNotFound(memberId));

        var memberships = await dashboardRepository.GetMembershipsAsync(memberId, ct);
        var meetings = await dashboardRepository.GetTeamMeetingsAsync(memberId, ct);
        var attendanceRows = await dashboardRepository.GetAttendanceRowsAsync(memberId, ct);
        var paymentRows = await dashboardRepository.GetPaymentRowsAsync(memberId, ct);
        var employeeRows = await dashboardRepository.GetEmployeeRowsAsync(memberId, ct);
        var departmentRows = await dashboardRepository.GetDepartmentRowsAsync(memberId, ct);

        var meetingsPerTeam = meetings
            .GroupBy(m => m.TeamId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.AttendanceDate).ToHashSet());

        var cards = memberships.Select(m => BuildCard(m, meetingsPerTeam, attendanceRows, paymentRows))
            .ToList();

        var service = BuildService(cards, employeeRows, departmentRows);

        return Result<MemberDashboardDto, MemberDashboardError>.Success(
            new MemberDashboardDto(member.Id, BuildProfile(member), cards.Count, cards, service));
    }

    private static MemberServiceSummaryDto BuildService(
        List<MemberTeamCardDto> cards,
        List<EmployeeServiceRow> employeeRows,
        List<DepartmentAssignmentRow> departmentRows)
    {
        var meetings = cards.Sum(c => c.Attendance.Meetings);
        var attended = cards.Sum(c => c.Attendance.Attended);
        var rate = meetings == 0 ? (double?)null : Math.Round(attended * 100.0 / meetings, 2);

        var departmentsByEmployee = departmentRows
            .GroupBy(d => d.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(d =>
                new MemberDepartmentAssignmentDto(d.DepartmentId, d.DepartmentName, d.Role)).ToList());

        var services = employeeRows.Select(e => new MemberServiceDto(
                e.Id,
                e.ChurchId is null ? "District Office" : e.ChurchName!,
                e.ChurchId,
                e.Position,
                e.EmploymentType,
                e.IsPresident,
                e.IsVicePresident,
                departmentsByEmployee.GetValueOrDefault(e.Id, [])))
            .ToList();

        return new MemberServiceSummaryDto(
            meetings, attended, rate, cards.Sum(c => c.Payments.TotalAmount), services);
    }

    private static MemberProfileDto BuildProfile(Member member) =>
        new(
            MemberNames.Full(member),
            member.EfgbcId,
            member.Church.Name,
            member.DateOfBirth,
            AgeOf(member.DateOfBirth),
            member.Gender,
            member.Phone,
            member.Email,
            member.Status,
            member.JoinedAt);

    private static int? AgeOf(DateOnly? dateOfBirth)
    {
        if (dateOfBirth is null)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Value.Year;

        if (today < dateOfBirth.Value.AddYears(age))
            age--;

        return age;
    }

    private static MemberTeamCardDto BuildCard(
        TeamMembershipRow membership,
        IReadOnlyDictionary<long, HashSet<DateOnly>> meetingsPerTeam,
        List<MemberAttendanceRow> attendanceRows,
        List<MemberPaymentRow> paymentRows)
    {
        var teamAttendance = attendanceRows
            .Where(a => a.TeamId == membership.TeamId)
            .ToList();

        var meetings = meetingsPerTeam.GetValueOrDefault(membership.TeamId, []);
        var present = teamAttendance.Count(a => a.Status == TeamAttendanceStatus.Present);
        var late = teamAttendance.Count(a => a.Status == TeamAttendanceStatus.Late);
        var absent = teamAttendance.Count(a => a.Status == TeamAttendanceStatus.Absent);

        double? rate = meetings.Count == 0
            ? null
            : Math.Round((present + late) * 100.0 / meetings.Count, 2);

        var teamPayments = paymentRows
            .Where(p => p.TeamId == membership.TeamId)
            .ToList();

        return new MemberTeamCardDto(
            membership.TeamId,
            membership.TeamName,
            membership.Role.ToString(),
            new MemberAttendanceSummaryDto(
                meetings.Count, present, late, absent, present + late,
                rate, teamAttendance.MaxBy(a => a.AttendanceDate)?.AttendanceDate),
            new MemberPaymentSummaryDto(
                teamPayments.Count,
                teamPayments.Sum(p => p.Amount),
                teamPayments.MaxBy(p => p.Month)?.Month));
    }
}