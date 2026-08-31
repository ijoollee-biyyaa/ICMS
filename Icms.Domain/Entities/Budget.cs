namespace Icms.Domain.Entities;

public class Budget
{
    public long Id { get; set; }
    public long? ChurchId { get; set; }
    public int Year { get; set; }
    public int? Quarter { get; set; }
    public decimal Amount { get; set; }
    public bool IsDeleted { get; set; }

    public Church? Church { get; set; }
}
