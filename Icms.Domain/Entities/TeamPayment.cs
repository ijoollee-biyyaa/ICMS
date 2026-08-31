namespace Icms.Domain.Entities;

public class TeamPayment
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public long MemberId { get; set; }
    public DateOnly? Month { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;

    public Team Team { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
