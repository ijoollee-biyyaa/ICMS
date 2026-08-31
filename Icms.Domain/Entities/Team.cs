using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Team
{
    public long Id { get; set; }
    public long ChurchId { get; set; }
    public long? ParentTeamId { get; set; }
    public MembershipRule MembershipRule { get; set; } = MembershipRule.Single;
    public required string Name { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Church Church { get; set; } = null!;
    public Team? ParentTeam { get; set; }
    public ICollection<Team> SubTeams { get; set; } = new List<Team>();
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamAttendance> Attendances { get; set; } = new List<TeamAttendance>();
    public ICollection<TeamPayment> Payments { get; set; } = new List<TeamPayment>();
    public ICollection<TeamActivity> Activities { get; set; } = new List<TeamActivity>();
}
