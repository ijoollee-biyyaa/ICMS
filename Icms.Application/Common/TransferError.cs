namespace Icms.Application.Common;

public sealed record TransferError(string Code, string Message, int Status) : IAppError
{
    public static TransferError NotFound(long id) =>
        new("transfer_not_found", $"Transfer request with id {id} was not found.", 404);

    public static TransferError ChurchNotFound(long churchId) =>
        new("transfer_church_not_found", $"Church with id {churchId} was not found.", 404);

    public static TransferError MemberNotFound(long memberId) =>
        new("transfer_member_not_found", $"Member with id {memberId} was not found.", 404);

    public static TransferError MemberNotActive(long memberId) =>
        new("transfer_member_not_active", $"Member with id {memberId} is not active and cannot be transferred.", 409);

    public static TransferError InvalidState(string reason) =>
        new("transfer_invalid_state", $"Cannot perform this action: {reason}.", 409);

    public static TransferError InternalError(string message) =>
        new("transfer_internal_error", $"An internal error occurred: {message}", 500);
}
