using Icms.Domain.Entities;

namespace Icms.Application.DTOs;

public record RecordPaymentRequest(
    long? MemberId,
    DateOnly? Month,
    decimal? Amount);

public record UpdatePaymentRequest(
    decimal? Amount);

public record TeamPaymentDto(
    long Id,
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    DateOnly Month,
    decimal Amount,
    DateTimeOffset PaidAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}

public record TeamPaymentSummaryDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    int PaymentCount,
    decimal TotalAmount,
    DateOnly? LastPaidMonth);