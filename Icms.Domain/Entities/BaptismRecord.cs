namespace Icms.Domain.Entities;

public class BaptismRecord
{
    public long Id { get; set; }
    public long? MemberId { get; set; }
    public long FatherMemberId { get; set; }
    public long MotherMemberId { get; set; }
    public long? MinisterEmployeeId { get; set; }
    public long? ChurchId { get; set; }
    public required string ChildName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? BaptismDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;

    public Member? Member { get; set; }
    public Member Father { get; set; } = null!;
    public Member Mother { get; set; } = null!;
    public Employee? Minister { get; set; }
    public Church? Church { get; set; }
}