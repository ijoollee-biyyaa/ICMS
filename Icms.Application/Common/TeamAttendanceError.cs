namespace Icms.Application.Common;

public sealed record TeamAttendanceError(string Code, string Message, int Status) : IAppError
{
    public static TeamAttendanceError TeamNotFound(long teamId) =>
        new("team_not_found", $"Team with id {teamId} was not found in this church.", 404);

    public static TeamAttendanceError TeamIsCategory(long teamId) =>
        new("team_is_category",
            "A main team with sub-teams is a category; attendance belongs to its sub-teams.", 409);

    public static TeamAttendanceError MemberNotInTeam(long memberId, long teamId) =>
        new("team_attendance_member_not_in_team",
            $"Member {memberId} is not an active member of team {teamId}.", 409);
}