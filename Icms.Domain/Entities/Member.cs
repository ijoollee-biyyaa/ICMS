using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Member
{
    public long Id { get; set; }
    public required string EfgbcId { get; set; }
    public long ChurchId { get; set; }
    public required string FirstName { get; set; }
    public required string FatherName { get; set; }
    public required string GrandfatherName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public JobStatus JobStatus { get; set; } = JobStatus.Other;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public DeactivationReason DeactivationReason { get; set; } = DeactivationReason.None;
    public JoinChannel JoinedVia { get; set; } = JoinChannel.Salvation;
    public DateOnly? JoinedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Church Church { get; set; } = null!;
    public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Tithe> Tithes { get; set; } = new List<Tithe>();
    public ICollection<MemberDocument> Documents { get; set; } = new List<MemberDocument>();
    public ICollection<BaptismRecord> Baptisms { get; set; } = new List<BaptismRecord>();
    public ICollection<LearningRecord> Learnings { get; set; } = new List<LearningRecord>();
    public DeathRecord? DeathRecord { get; set; }
    public ICollection<PrayerRequest> PrayerRequests { get; set; } = new List<PrayerRequest>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
