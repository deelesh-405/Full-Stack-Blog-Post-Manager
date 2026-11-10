using BlogManager.API.Enums;

namespace BlogManager.API.Models;

public class BlogPost
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string? FeaturedImageUrl { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
