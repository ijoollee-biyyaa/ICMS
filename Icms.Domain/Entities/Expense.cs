namespace Icms.Domain.Entities;

public class Expense
{
    public long Id { get; set; }
    public long? ChurchId { get; set; }
    public required string Category { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly? ExpenseDate { get; set; }
    public string? ApprovedById { get; set; }
    public bool IsDeleted { get; set; }

    public Church? Church { get; set; }
}
