namespace Icms.Application.Common;

public sealed record MemberError(string Code, string Message, int Status) : IAppError
{
    public static MemberError NotFound(long id) =>
        new("member_not_found", $"Member with id {id} was not found.", 404);

    public static MemberError ChurchNotFound(long churchId) =>
        new("member_church_not_found", $"Church with id {churchId} was not found.", 409);

    public static MemberError ReferencedChurchNotFound(long churchId) =>
        new("church_not_found", $"Church with id {churchId} was not found.", 404);

    public static MemberError EfgbcIdAlreadyExists(string efgbcId) =>
        new("member_efgbc_id_exists", $"Member with EFGBC ID '{efgbcId}' already exists.", 409);

    public static MemberError AccountCreationFailed(string errors) =>
        new("member_account_creation_failed", $"Could not create the member login account: {errors}.", 400);
}