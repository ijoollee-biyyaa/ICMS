using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class PrayerRequest
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public required string Content { get; set; }
    public PrayerRequestStatus Status { get; set; } = PrayerRequestStatus.Pending;
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? RespondedById { get; set; }
    public string? Response { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }

    public Member Member { get; set; } = null!;
}
