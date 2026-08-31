using Icms.Domain.Enums;

namespace Icms.Application.Common;

public sealed record TeamMemberError(string Code, string Message, int Status) : IAppError
{
    public static TeamMemberError TeamNotFound(long teamId) =>
        new("team_not_found", $"Team with id {teamId} was not found.", 404);

    public static TeamMemberError MemberNotFound(long memberId) =>
        new("team_member_not_found", $"Member with id {memberId} was not found.", 409);

    public static TeamMemberError AlreadyMember(long teamId, long memberId) =>
        new("team_member_exists", $"Member {memberId} is already a member of team {teamId}.", 409);

    public static TeamMemberError NotAMember(long teamId, long memberId) =>
        new("team_not_a_member", $"Member {memberId} is not a member of team {teamId}.", 404);

    public static TeamMemberError MemberChurchMismatch(long memberId, long churchId) =>
        new("team_member_wrong_church",
            $"Member {memberId} belongs to a different church than this team.", 409);

    public static TeamMemberError TeamIsCategory(long teamId) =>
        new("team_is_category",
            "A main team with sub-teams is a category; add the member to one of its sub-teams.", 409);

    public static TeamMemberError SingleMembershipLimit() =>
        new("team_member_single_family",
            "A member can belong to only one team in this family.", 409);
}