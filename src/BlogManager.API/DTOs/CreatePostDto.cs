using System.ComponentModel.DataAnnotations;
using BlogManager.API.Enums;

namespace BlogManager.API.DTOs;

public class CreatePostDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Excerpt { get; set; }

    [Url]
    public string? FeaturedImageUrl { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;
}
