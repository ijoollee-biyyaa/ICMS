using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class UpgradeApplication
{
    public long Id { get; set; }
    public long DaughterChurchId { get; set; }
    public UpgradeStatus Status { get; set; } = UpgradeStatus.Submitted;
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? DecidedById { get; set; }
    public DateTimeOffset? DecisionAt { get; set; }
    public string? Feedback { get; set; }
    public string? Conditions { get; set; }
    public DateOnly? ConditionDeadline { get; set; }

    public Church DaughterChurch { get; set; } = null!;
}
