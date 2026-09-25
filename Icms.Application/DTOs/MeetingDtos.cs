using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateMeetingRequest(
    DateOnly? MeetingDate,
    string? Title,
    string? Notes);

public record TeamMeetingDto(
    long Id,
    long TeamId,
    DateOnly MeetingDate,
    string? Title,
    string? Notes,
    long? CreatedById,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    int TotalMembers,
    int Present,
    int Late,
    int Absent,
    int Marked);

public record TeamMeetingDetailDto(
    long Id,
    long TeamId,
    DateOnly MeetingDate,
    string? Title,
    string? Notes,
    long? CreatedById,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TeamMeetingAttendanceDto> Attendance);

public record TeamMeetingAttendanceDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    TeamAttendanceStatus? Status,
    string? Reason);

public record TeamMeetingRecordDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    long MeetingId,
    DateOnly MeetingDate,
    TeamAttendanceStatus Status,
    string? Reason);