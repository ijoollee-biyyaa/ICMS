namespace Icms.Domain.Entities;

public class TeamMeeting
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public DateOnly MeetingDate { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public long? CreatedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Team Team { get; set; } = null!;
    public Member? CreatedBy { get; set; }
    public ICollection<TeamAttendance> Attendances { get; set; } = new List<TeamAttendance>();
}