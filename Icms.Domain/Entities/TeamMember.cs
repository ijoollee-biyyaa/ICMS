using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class TeamMember
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public long MemberId { get; set; }
    public TeamMemberRole Role { get; set; } = TeamMemberRole.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }

    public Team Team { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
