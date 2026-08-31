namespace Icms.Application.Common;

public sealed record TeamPaymentError(string Code, string Message, int Status) : IAppError
{
    public static TeamPaymentError TeamNotFound(long teamId) =>
        new("team_not_found", $"Team with id {teamId} was not found in this church.", 404);

    public static TeamPaymentError TeamIsCategory(long teamId) =>
        new("team_is_category",
            "A main team with sub-teams is a category; payments belong to its sub-teams.", 409);

    public static TeamPaymentError MemberNotInTeam(long memberId, long teamId) =>
        new("team_payment_member_not_in_team",
            $"Member {memberId} is not an active member of team {teamId}.", 409);

    public static TeamPaymentError PaymentExists(long memberId, DateOnly month) =>
        new("team_payment_exists",
            $"A payment for member {memberId} in {month:yyyy-MM} is already recorded.", 409);

    public static TeamPaymentError PaymentNotFound(long paymentId) =>
        new("team_payment_not_found",
            $"Payment with id {paymentId} was not found in this team.", 404);
}