namespace Icms.Application.Common;

public sealed record TeamMeetingError(string Code, string Message, int Status) : IAppError
{
    public static TeamMeetingError TeamNotFound(long teamId) =>
        new("team_not_found", $"Team with id {teamId} was not found in this church.", 404);

    public static TeamMeetingError MeetingNotFound(long meetingId) =>
        new("meeting_not_found", $"Meeting with id {meetingId} was not found for this team.", 404);

    public static TeamMeetingError TeamIsCategory(long teamId) =>
        new("team_is_category",
            "A main team with sub-teams is a category; meetings belong to its sub-teams.", 409);

    public static TeamMeetingError MemberNotInTeam(long memberId, long teamId) =>
        new("team_attendance_member_not_in_team",
            $"Member {memberId} is not an active member of team {teamId}.", 409);

    public static TeamMeetingError MeetingNotFromTeam(long meetingId, long teamId) =>
        new("meeting_not_from_team",
            $"Meeting {meetingId} does not belong to team {teamId}.", 409);
}