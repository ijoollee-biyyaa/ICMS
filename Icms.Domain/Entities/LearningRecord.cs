namespace Icms.Domain.Entities;

public class LearningRecord
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public int Status { get; set; }

    public Member Member { get; set; } = null!;
}
