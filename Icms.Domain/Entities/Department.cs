using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Department
{
    public long Id { get; set; }
    public long? ChurchId { get; set; }
    public required string Name { get; set; }
    public DepartmentType Type { get; set; } = DepartmentType.Spiritual;
    public long? HeadEmployeeId { get; set; }
    public bool IsDeleted { get; set; }

    public Church? Church { get; set; }
    public Employee? HeadEmployee { get; set; }
    public ICollection<DepartmentEmployee> Employees { get; set; } = new List<DepartmentEmployee>();
    public ICollection<DepartmentActivity> Activities { get; set; } = new List<DepartmentActivity>();
}
