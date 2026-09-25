namespace Icms.Domain.Entities;

public class TransferSnapshot
{
    public long Id { get; set; }
    public long TransferId { get; set; }
    public long MemberId { get; set; }

    public string TeamsJson { get; set; } = "[]";
    public string DepartmentsJson { get; set; } = "[]";
    public string PaymentSummaryJson { get; set; } = "{}";
    public string AttendanceSummaryJson { get; set; } = "{}";
    public DateTimeOffset SnapshotAt { get; set; } = DateTimeOffset.UtcNow;

    public Transfer Transfer { get; set; } = null!;
}
