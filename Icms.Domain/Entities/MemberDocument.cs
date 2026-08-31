using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class MemberDocument
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public DocumentType Type { get; set; } = DocumentType.Other;
    public required string FileUrl { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public Member Member { get; set; } = null!;
}
