namespace Icms.Domain.Entities;

public class BlogPost
{
    public long Id { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public bool IsDeleted { get; set; }

}
