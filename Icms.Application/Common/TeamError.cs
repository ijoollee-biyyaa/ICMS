namespace Icms.Application.Common;

public sealed record TeamError(string Code, string Message, int Status) : IAppError
{
    public static TeamError NotFound(long id) =>
        new("team_not_found", $"Team with id {id} was not found in this church.", 404);

    public static TeamError ChurchNotFound(long churchId) =>
        new("church_not_found", $"Church with id {churchId} was not found.", 404);

    public static TeamError ParentNotFound(long parentId) =>
        new("team_parent_not_found",
            $"Parent team with id {parentId} was not found in this church.", 409);

    public static TeamError ParentIsSubTeam() =>
        new("team_parent_is_sub_team", "Only a main team can have sub-teams.", 409);

    public static TeamError ParentHasMembers() =>
        new("team_parent_has_members",
            "Cannot create a sub-team under a main team that already has members.", 409);

    public static TeamError NameAlreadyExists(string name) =>
        new("team_name_exists", $"A team named '{name}' already exists in this church.", 409);

    public static TeamError CannotDeleteWithSubTeams() =>
        new("team_delete_has_sub_teams", "Cannot delete a team that has sub-teams.", 409);

    public static TeamError CannotDeleteWithMembers() =>
        new("team_delete_has_members", "Cannot delete a team that has members.", 409);
}
