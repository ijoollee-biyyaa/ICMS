using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class TeamAttendance
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public long MeetingId { get; set; }
    public long MemberId { get; set; }
    public DateOnly? AttendanceDate { get; set; }
    public TeamAttendanceStatus Status { get; set; }
    public string? Reason { get; set; }

    public Team Team { get; set; } = null!;
    public TeamMeeting Meeting { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
