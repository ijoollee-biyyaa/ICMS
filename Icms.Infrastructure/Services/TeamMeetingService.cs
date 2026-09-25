using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Application.Teams;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Services;

public class TeamMeetingService(
    ITeamMeetingRepository meetingRepository,
    CreateMeetingValidator createValidator,
    SaveAttendanceValidator saveValidator,
    ILogger<TeamMeetingService> logger) : ITeamMeetingService
{
    public async Task<Result<TeamMeetingDto, TeamMeetingError>> CreateMeetingAsync(
        long churchId, long teamId, CreateMeetingRequest request, long createdById, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var team = await meetingRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMeetingDto, TeamMeetingError>.Failure(
                TeamMeetingError.TeamNotFound(teamId));

        if (team.ParentTeamId is null
            && await meetingRepository.HasSubTeamsAsync(churchId, teamId, ct))
            return Result<TeamMeetingDto, TeamMeetingError>.Failure(
                TeamMeetingError.TeamIsCategory(teamId));

        var meeting = new TeamMeeting
        {
            TeamId = teamId,
            MeetingDate = request.MeetingDate!.Value,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedById = createdById,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await meetingRepository.CreateAsync(meeting, ct);

        logger.LogInformation("Created meeting {MeetingId} for team {TeamId} on {Date}",
            meeting.Id, teamId, meeting.MeetingDate);

        return Result<TeamMeetingDto, TeamMeetingError>.Success(ToDto(meeting));
    }

    public async Task<Result<PagedResponse<TeamMeetingDto>, TeamMeetingError>> GetMeetingsAsync(
        long churchId, long teamId, PagedRequest paging, CancellationToken ct)
    {
        var team = await meetingRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<PagedResponse<TeamMeetingDto>, TeamMeetingError>.Failure(
                TeamMeetingError.TeamNotFound(teamId));

        var page = paging.SafePage;
        var meetings = await meetingRepository.ListMeetingsAsync(
            churchId, teamId, page, paging.PageSize, ct);
        var totalCount = await meetingRepository.CountMeetingsAsync(churchId, teamId, ct);

        var dtos = new List<TeamMeetingDto>();
        foreach (var meeting in meetings)
        {
            var counts = await MeetingCountsAsync(meeting.Id, ct);
            dtos.Add(ToDto(meeting, counts));
        }

        return Result<PagedResponse<TeamMeetingDto>, TeamMeetingError>.Success(
            new PagedResponse<TeamMeetingDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<TeamMeetingDetailDto, TeamMeetingError>> GetMeetingAsync(
        long churchId, long teamId, long meetingId, CancellationToken ct)
    {
        var team = await meetingRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                TeamMeetingError.TeamNotFound(teamId));

        var meeting = await meetingRepository.GetByIdWithAttendanceAsync(teamId, meetingId, ct);
        if (meeting is null)
            return Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                TeamMeetingError.MeetingNotFound(meetingId));

        return Result<TeamMeetingDetailDto, TeamMeetingError>.Success(ToDetailDto(meeting));
    }

    public async Task<Result<TeamMeetingDetailDto, TeamMeetingError>> SaveAttendanceAsync(
        long churchId, long teamId, long meetingId, SaveAttendanceRequest request, CancellationToken ct)
    {
        await saveValidator.ValidateAndThrowAsync(request, ct);

        var team = await meetingRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                TeamMeetingError.TeamNotFound(teamId));

        var meeting = await meetingRepository.GetByIdAsync(teamId, meetingId, ct);
        if (meeting is null)
            return Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                TeamMeetingError.MeetingNotFound(meetingId));

        var activeMemberIds = (await meetingRepository.GetActiveMemberIdsAsync(churchId, teamId, ct))
            .ToHashSet();

        foreach (var entry in request.Entries!)
        {
            if (!activeMemberIds.Contains(entry.MemberId!.Value))
                return Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                    TeamMeetingError.MemberNotInTeam(entry.MemberId.Value, teamId));
        }

        await meetingRepository.UpsertAttendanceAsync(
            meetingId, teamId, meeting.MeetingDate, request.Entries
                .Select(e => (e.MemberId!.Value, e.Status!.Value, e.Reason))
                .ToList(), ct);

        var updated = await meetingRepository.GetByIdWithAttendanceAsync(teamId, meetingId, ct);

        logger.LogInformation("Saved attendance for meeting {MeetingId} of team {TeamId}",
            meetingId, teamId);

        return updated is null
            ? Result<TeamMeetingDetailDto, TeamMeetingError>.Failure(
                TeamMeetingError.MeetingNotFound(meetingId))
            : Result<TeamMeetingDetailDto, TeamMeetingError>.Success(ToDetailDto(updated));
    }

    public async Task<Result<TeamMeetingDto, TeamMeetingError>> DeleteMeetingAsync(
        long churchId, long teamId, long meetingId, CancellationToken ct)
    {
        var team = await meetingRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMeetingDto, TeamMeetingError>.Failure(
                TeamMeetingError.TeamNotFound(teamId));

        var meeting = await meetingRepository.GetByIdAsync(teamId, meetingId, ct);
        if (meeting is null)
            return Result<TeamMeetingDto, TeamMeetingError>.Failure(
                TeamMeetingError.MeetingNotFound(meetingId));

        var dto = ToDto(meeting);

        await meetingRepository.DeleteAsync(meeting, ct);

        logger.LogInformation("Deleted meeting {MeetingId} of team {TeamId} with its attendance",
            meetingId, teamId);

        return Result<TeamMeetingDto, TeamMeetingError>.Success(dto);
    }

    private async Task<(int Total, int Present, int Late, int Absent, int Marked)> MeetingCountsAsync(
        long meetingId, CancellationToken ct)
    {
        var rows = await meetingRepository.GetCountsAsync(meetingId, ct);
        var present = rows.Count(r => r.Status == TeamAttendanceStatus.Present);
        var late = rows.Count(r => r.Status == TeamAttendanceStatus.Late);
        var absent = rows.Count(r => r.Status == TeamAttendanceStatus.Absent);
        return (rows.Count, present, late, absent, present + late + absent);
    }

    private static TeamMeetingDto ToDto(TeamMeeting m) =>
        new(m.Id, m.TeamId, m.MeetingDate, m.Title, m.Notes, m.CreatedById,
            m.CreatedBy is null ? null : MemberNames.Full(m.CreatedBy), m.CreatedAt,
            0, 0, 0, 0, 0);

    private static TeamMeetingDto ToDto(TeamMeeting m, (int Total, int Present, int Late, int Absent, int Marked) c) =>
        new(m.Id, m.TeamId, m.MeetingDate, m.Title, m.Notes, m.CreatedById,
            m.CreatedBy is null ? null : MemberNames.Full(m.CreatedBy), m.CreatedAt,
            c.Total, c.Present, c.Late, c.Absent, c.Marked);

    private static TeamMeetingDetailDto ToDetailDto(TeamMeeting m)
    {
        var attendance = (m.Attendances ?? new List<TeamAttendance>())
            .OrderBy(a => a.Member?.FirstName)
            .Select(a => new TeamMeetingAttendanceDto(
                a.MemberId,
                a.Member is null ? string.Empty : MemberNames.Full(a.Member),
                a.Member?.EfgbcId ?? string.Empty,
                a.Status,
                a.Reason))
            .ToList();

        return new TeamMeetingDetailDto(
            m.Id, m.TeamId, m.MeetingDate, m.Title, m.Notes, m.CreatedById,
            m.CreatedBy is null ? null : MemberNames.Full(m.CreatedBy), m.CreatedAt,
            attendance);
    }
}