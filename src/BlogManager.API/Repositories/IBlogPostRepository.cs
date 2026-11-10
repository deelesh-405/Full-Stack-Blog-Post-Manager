using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using BlogManager.API.Models;

namespace BlogManager.API.Repositories;

public interface IBlogPostRepository
{
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<PagedResult<BlogPost>> GetPagedAsync(int page, int pageSize, string? search, PostStatus? status);
    Task AddAsync(BlogPost post);
    Task UpdateAsync(BlogPost post);
    Task DeleteAsync(BlogPost post);
    Task<int> CountAsync();
}
