using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Application.Teams;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Services;

public class TeamAttendanceService(
    ITeamAttendanceRepository attendanceRepository,
    ILogger<TeamAttendanceService> logger) : ITeamAttendanceService
{
    public async Task<Result<PagedResponse<TeamAttendanceRecordDto>, TeamAttendanceError>> GetAttendanceAsync(
        long churchId, long teamId, DateOnly? from, DateOnly? to,
        PagedRequest paging, CancellationToken ct)
    {
        var team = await attendanceRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<PagedResponse<TeamAttendanceRecordDto>, TeamAttendanceError>.Failure(
                TeamAttendanceError.TeamNotFound(teamId));

        var page = paging.SafePage;

        var records = await attendanceRepository.GetByTeamAsync(
            churchId, teamId, from, to, paging.Search, page, paging.PageSize, ct);
        var totalCount = await attendanceRepository.CountByTeamAsync(
            churchId, teamId, from, to, paging.Search, ct);

        return Result<PagedResponse<TeamAttendanceRecordDto>, TeamAttendanceError>.Success(
            new PagedResponse<TeamAttendanceRecordDto>
            {
                Items = records.Select(ToRecordDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<TeamAttendanceReportDto, TeamAttendanceError>> GetSummaryAsync(
        long churchId, long teamId, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var team = await attendanceRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamAttendanceReportDto, TeamAttendanceError>.Failure(
                TeamAttendanceError.TeamNotFound(teamId));

        var rows = await attendanceRepository.GetSummaryRowsAsync(churchId, teamId, from, to, ct);

        var totalMeetings = rows
            .Where(a => a.AttendanceDate is not null)
            .Select(a => a.AttendanceDate!.Value)
            .Distinct()
            .Count();

        var members = rows
            .GroupBy(a => a.MemberId)
            .Select(g =>
            {
                var present = g.Count(a => a.Status == TeamAttendanceStatus.Present);
                var late = g.Count(a => a.Status == TeamAttendanceStatus.Late);
                var absent = g.Count(a => a.Status == TeamAttendanceStatus.Absent);
                var attended = present + late;
                var total = g.Count();

                return new TeamAttendanceSummaryDto(
                    g.Key,
                    MemberNames.Full(g.First().Member),
                    g.First().Member.EfgbcId,
                    total,
                    present,
                    late,
                    absent,
                    attended,
                    Math.Round(total == 0 ? 0 : (double)attended / total * 100, 2));
            })
            .OrderByDescending(s => s.AttendanceRate)
            .ThenBy(s => s.MemberName)
            .ToList();

        var totalPresent = members.Sum(m => m.Present);
        var totalLate = members.Sum(m => m.Late);
        var totalAbsent = members.Sum(m => m.Absent);
        var totalMarks = totalPresent + totalLate + totalAbsent;

        var report = new TeamAttendanceReportDto(
            teamId,
            team.Name,
            from,
            to,
            totalMeetings,
            totalPresent,
            totalLate,
            totalAbsent,
            Math.Round(totalMarks == 0 ? 0 : (double)(totalPresent + totalLate) / totalMarks * 100, 2),
            members);

        return Result<TeamAttendanceReportDto, TeamAttendanceError>.Success(report);
    }

    private static TeamAttendanceRecordDto ToRecordDto(TeamAttendance a) =>
        new(a.MemberId, MemberNames.Full(a.Member), a.Member.EfgbcId, a.AttendanceDate!.Value, a.Status, a.Reason);
}