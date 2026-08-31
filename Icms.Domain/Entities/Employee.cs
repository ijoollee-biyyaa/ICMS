using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Employee
{
    public long Id { get; set; }
    public long? ChurchId { get; set; }
    public long? DistrictId { get; set; }
    public string? UserId { get; set; }
    public long? MemberId { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public MinisterTitle? MinisterTitle { get; set; }
    public required string Position { get; set; }
    public string? FirstName { get; set; }
    public string? FatherName { get; set; }
    public string? GrandfatherName { get; set; }
    public bool IsDistrictPresident { get; set; }
    public bool IsVicePresident { get; set; }
    public DateOnly? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public SalaryPaidBy SalaryPaidBy { get; set; } = SalaryPaidBy.Church;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public bool IsDeleted { get; set; }

    public Church? Church { get; set; }
    public District? District { get; set; }
    public Member? Member { get; set; }
    public ICollection<Department> HeadedDepartments { get; set; } = new List<Department>();
}
