namespace BlogManager.API.DTOs;

public class DashboardSummaryDto
{
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public List<PostResponseDto> RecentPosts { get; set; } = [];
}
