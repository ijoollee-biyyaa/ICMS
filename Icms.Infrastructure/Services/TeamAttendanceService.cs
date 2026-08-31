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
    SaveAttendanceValidator saveValidator,
    ILogger<TeamAttendanceService> logger) : ITeamAttendanceService
{
    public async Task<Result<SaveAttendanceResult, TeamAttendanceError>> SaveAttendanceAsync(
        long churchId, long teamId, SaveAttendanceRequest request, CancellationToken ct)
    {
        await saveValidator.ValidateAndThrowAsync(request, ct);

        var team = await attendanceRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<SaveAttendanceResult, TeamAttendanceError>.Failure(
                TeamAttendanceError.TeamNotFound(teamId));

        if (team.ParentTeamId is null
            && await attendanceRepository.HasSubTeamsAsync(churchId, teamId, ct))
            return Result<SaveAttendanceResult, TeamAttendanceError>.Failure(
                TeamAttendanceError.TeamIsCategory(teamId));

        var activeMemberIds = (await attendanceRepository.GetActiveMemberIdsAsync(churchId, teamId, ct))
            .ToHashSet();

        foreach (var entry in request.Entries!)
        {
            if (!activeMemberIds.Contains(entry.MemberId!.Value))
                return Result<SaveAttendanceResult, TeamAttendanceError>.Failure(
                    TeamAttendanceError.MemberNotInTeam(entry.MemberId.Value, teamId));
        }

        var date = request.AttendanceDate!.Value;

        var entries = request.Entries
            .Select(e => (MemberId: e.MemberId!.Value, Status: e.Status!.Value, Reason: e.Reason))
            .ToList();

        await attendanceRepository.UpsertAsync(teamId, date, entries, ct);

        var present = entries.Count(e => e.Status == TeamAttendanceStatus.Present);
        var late = entries.Count(e => e.Status == TeamAttendanceStatus.Late);
        var absent = entries.Count(e => e.Status == TeamAttendanceStatus.Absent);

        logger.LogInformation("Saved attendance for team {TeamId} on {Date}: {Present} present, {Late} late, {Absent} absent",
            teamId, date, present, late, absent);

        return Result<SaveAttendanceResult, TeamAttendanceError>.Success(
            new SaveAttendanceResult(date, entries.Count, present, late, absent));
    }

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