using Microsoft.EntityFrameworkCore;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class MemberDashboardRepository(IcmsDbContext dbContext) : IMemberDashboardRepository
{
    public Task<Member?> GetMemberAsync(long memberId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .Include(m => m.Church)
            .SingleOrDefaultAsync(m => m.Id == memberId, ct);

    public Task<List<TeamMembershipRow>> GetMembershipsAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .Where(tm => tm.MemberId == memberId && !tm.IsDeleted && !tm.Team.IsDeleted)
            .OrderBy(tm => tm.Team.Name)
            .Select(tm => new TeamMembershipRow(tm.TeamId, tm.Team.Name, tm.Role, tm.Team.ChurchId))
            .ToListAsync(ct);

    public Task<List<TeamMeetingRow>> GetTeamMeetingsAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Where(a => a.MemberId == memberId && !a.Team.IsDeleted)
            .Select(a => new TeamMeetingRow(a.TeamId, a.AttendanceDate!.Value))
            .Distinct()
            .ToListAsync(ct);

    public Task<List<MemberAttendanceRow>> GetAttendanceRowsAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Where(a => a.MemberId == memberId && !a.Team.IsDeleted)
            .Select(a => new MemberAttendanceRow(a.TeamId, a.AttendanceDate!.Value, a.Status))
            .ToListAsync(ct);

    public Task<List<MemberPaymentRow>> GetPaymentRowsAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .Where(p => p.MemberId == memberId && !p.Team.IsDeleted)
            .Select(p => new MemberPaymentRow(p.TeamId, p.Month!.Value, p.Amount))
            .ToListAsync(ct);

    public Task<List<MemberAttendanceHistoryRow>> GetAttendanceHistoryAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Where(a => a.MemberId == memberId && !a.Team.IsDeleted)
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new MemberAttendanceHistoryRow(
                a.TeamId, a.Team.Name, a.AttendanceDate!.Value, a.Status, a.Reason))
            .ToListAsync(ct);

    public Task<List<MemberPaymentHistoryRow>> GetPaymentHistoryAsync(long memberId, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .Where(p => p.MemberId == memberId && !p.Team.IsDeleted)
            .OrderByDescending(p => p.Month)
            .Select(p => new MemberPaymentHistoryRow(
                p.Id, p.TeamId, p.Team.Name, p.Month!.Value, p.Amount, p.PaidAt))
            .ToListAsync(ct);

    public Task<List<EmployeeServiceRow>> GetEmployeeRowsAsync(long memberId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking()
            .Where(e => e.MemberId == memberId)
            .OrderBy(e => e.ChurchId.HasValue)
            .ThenBy(e => e.Position)
            .Select(e => new EmployeeServiceRow(
                e.Id, e.Position, e.EmploymentType,
                e.IsDistrictPresident, e.IsVicePresident, e.ChurchId,
                e.ChurchId != null ? e.Church.Name : null))
            .ToListAsync(ct);

    public Task<List<DepartmentAssignmentRow>> GetDepartmentRowsAsync(long memberId, CancellationToken ct) =>
        dbContext.DepartmentEmployees.AsNoTracking()
            .Where(de => de.Employee.MemberId == memberId)
            .OrderBy(de => de.Department.Name)
            .Select(de => new DepartmentAssignmentRow(
                de.EmployeeId, de.DepartmentId, de.Department.Name, de.Role))
            .ToListAsync(ct);
}