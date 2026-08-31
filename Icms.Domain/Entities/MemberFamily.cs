namespace Icms.Domain.Entities;

public class MemberFamily
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public required string FamilyName { get; set; }
    public long HeadOfHouseholdMemberId { get; set; }

    public Member Member { get; set; } = null!;
    public Member HeadOfHousehold { get; set; } = null!;
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
}
