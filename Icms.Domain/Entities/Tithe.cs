using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Tithe
{
    public long Id { get; set; }
    public long ChurchId { get; set; }
    public long? MemberId { get; set; }
    public FinanceType Type { get; set; } = FinanceType.Tithe;
    public decimal Amount { get; set; }
    public DateOnly? IncomeDate { get; set; }
    public string? RecordedById { get; set; }

    public Church Church { get; set; } = null!;
    public Member? Member { get; set; }
}
