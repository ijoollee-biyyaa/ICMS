using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record SaveAttendanceRequest(
    long MeetingId,
    List<AttendanceEntryRequest>? Entries);

public record AttendanceEntryRequest(
    long? MemberId,
    TeamAttendanceStatus? Status,
    string? Reason);

public record TeamAttendanceRecordDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    DateOnly AttendanceDate,
    TeamAttendanceStatus Status,
    string? Reason);

public record TeamAttendanceSummaryDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    int TotalMeetings,
    int Present,
    int Late,
    int Absent,
    int Attended,
    double AttendanceRate);

public record TeamAttendanceReportDto(
    long TeamId,
    string TeamName,
    DateOnly? From,
    DateOnly? To,
    int TotalMeetings,
    int TotalPresent,
    int TotalLate,
    int TotalAbsent,
    double OverallRate,
    List<TeamAttendanceSummaryDto> Members);

public record SaveAttendanceResult(
    DateOnly AttendanceDate,
    int TotalMarked,
    int PresentCount,
    int LateCount,
    int AbsentCount);