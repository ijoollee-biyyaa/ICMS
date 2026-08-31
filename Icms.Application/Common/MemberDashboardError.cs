namespace Icms.Application.Common;

public sealed record MemberDashboardError(string Code, string Message, int Status) : IAppError
{
    public static MemberDashboardError MemberNotFound(long memberId) =>
        new("member_not_found", $"Member with id {memberId} was not found.", 404);
}