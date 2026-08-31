namespace Icms.Domain.Entities;

public class DeathRecord
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public DateOnly? DeathDate { get; set; }
    public long? ChurchId { get; set; }
    public string? RecordedBy { get; set; }

    public Member Member { get; set; } = null!;
    public Church? Church { get; set; }
}
