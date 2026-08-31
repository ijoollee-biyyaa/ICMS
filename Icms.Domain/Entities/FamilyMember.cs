using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class FamilyMember
{
    public long Id { get; set; }
    public long FamilyId { get; set; }
    public long MemberId { get; set; }
    public FamilyRelation Relation { get; set; } = FamilyRelation.Other;

    public MemberFamily Family { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
