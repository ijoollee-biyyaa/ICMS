namespace Icms.Application.Common;

public sealed record ChurchError(string Code, string Message, int Status) : IAppError
{
    public static ChurchError CodeAlreadyExists(string code) =>
        new("church_code_exists", $"Church code '{code}' already exists.", 409);

    public static ChurchError DistrictNotFound(long districtId) =>
        new("district_not_found", $"District with id {districtId} was not found.", 404);

    public static ChurchError NotFound(long id) =>
        new("church_not_found", $"Church with id {id} was not found.", 404);

    public static ChurchError ParentNotFound(long parentId) =>
        new("church_parent_not_found", $"Parent church with id {parentId} was not found.", 409);

    public static ChurchError ParentNotLocal() =>
        new("church_parent_not_local", "Only a local church can have daughter churches.", 409);

    public static ChurchError CannotDeleteWithDaughters() =>
        new("church_delete_has_daughters", "Cannot delete a church that has daughter churches.", 409);

    public static ChurchError CannotDeleteWithMembers() =>
        new("church_delete_has_members", "Cannot delete a church that has members.", 409);

    public static ChurchError AdminEmailAlreadyExists(string email) =>
        new("church_admin_email_exists", $"An account with email '{email}' already exists.", 409);

    public static ChurchError AdminCreationFailed(string message) =>
        new("church_admin_create_failed", $"Church admin account could not be created: {message}", 409);

    public static ChurchError AdminMemberNotFound(long memberId) =>
        new("church_admin_member_not_found", $"Member with id {memberId} was not found.", 404);

    public static ChurchError AdminMemberNotInChurch() =>
        new("church_admin_member_wrong_church", "The selected member does not belong to this church.", 409);
}