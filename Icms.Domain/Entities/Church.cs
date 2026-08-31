using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Church
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public required string Name { get; set; }
    public ChurchType Type { get; set; } = ChurchType.Local;
    public long? ParentChurchId { get; set; }
    public required string Code { get; set; }
    public string? City { get; set; }
    public string? Subcity { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Tel { get; set; }
    public string? MapAddress { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public District District { get; set; } = null!;
    public Church? ParentChurch { get; set; }
    public ICollection<Church> ChildChurches { get; set; } = new List<Church>();
    public ICollection<ServiceTime> ServiceTimes { get; set; } = new List<ServiceTime>();
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Transfer> OutgoingTransfers { get; set; } = new List<Transfer>();
    public ICollection<Transfer> IncomingTransfers { get; set; } = new List<Transfer>();
}
