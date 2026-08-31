namespace Icms.Domain.Entities;

public class ServiceTime
{
    public long Id { get; set; }
    public long ChurchId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? ServiceType { get; set; }

    public Church Church { get; set; } = null!;
}
