using System.ComponentModel.DataAnnotations;
using BlogManager.API.Enums;

namespace BlogManager.API.DTOs;

public class UpdatePostDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    public string? Content { get; set; }

    [StringLength(500)]
    public string? Excerpt { get; set; }

    [Url]
    public string? FeaturedImageUrl { get; set; }

    public PostStatus? Status { get; set; }
}
