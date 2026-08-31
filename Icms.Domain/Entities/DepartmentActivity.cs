namespace Icms.Domain.Entities;

public class DepartmentActivity
{
    public long Id { get; set; }
    public long DepartmentId { get; set; }
    public required string Title { get; set; }
    public DateOnly? ActivityDate { get; set; }
    public string? Description { get; set; }

    public Department Department { get; set; } = null!;
}
