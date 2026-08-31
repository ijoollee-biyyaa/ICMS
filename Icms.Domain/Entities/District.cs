namespace Icms.Domain.Entities;

public class District
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Address { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Church> Churches { get; set; } = new List<Church>();
}
