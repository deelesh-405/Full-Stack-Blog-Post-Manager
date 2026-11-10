using BlogManager.API.DTOs;
using BlogManager.API.Enums;

namespace BlogManager.API.Services;

public interface IBlogPostService
{
    Task<PagedResult<PostResponseDto>> GetPostsAsync(int page, int pageSize, string? search, PostStatus? status);
    Task<PostResponseDto?> GetByIdAsync(int id);
    Task<PostResponseDto> CreateAsync(CreatePostDto dto);
    Task<PostResponseDto?> UpdateAsync(int id, UpdatePostDto dto);
    Task<bool> DeleteAsync(int id);
    Task<DashboardSummaryDto> GetDashboardAsync();
}
