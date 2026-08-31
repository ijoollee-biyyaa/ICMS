namespace Icms.Domain.Entities;

public class TeamActivity
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public required string Title { get; set; }
    public DateOnly? ActivityDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public Team Team { get; set; } = null!;
}
