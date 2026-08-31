using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class DepartmentEmployee
{
    public long Id { get; set; }
    public long DepartmentId { get; set; }
    public long EmployeeId { get; set; }
    public DepartmentEmployeeRole Role { get; set; } = DepartmentEmployeeRole.Staff;

    public Department Department { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}