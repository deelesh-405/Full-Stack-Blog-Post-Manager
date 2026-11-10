using BlogManager.API.Data;
using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using BlogManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManager.API.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly ApplicationDbContext _context;

    public BlogPostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        return await _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        return await _context.Posts.FirstOrDefaultAsync(post => post.Slug == slug);
    }

    public async Task<PagedResult<BlogPost>> GetPagedAsync(int page, int pageSize, string? search, PostStatus? status)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var query = _context.Posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            var lowered = normalized.ToLower();
            query = query.Where(post =>
                post.Title.ToLower().Contains(lowered) ||
                post.Content.ToLower().Contains(lowered) ||
                (post.Excerpt != null && post.Excerpt.ToLower().Contains(lowered)));
        }

        if (status.HasValue)
        {
            query = query.Where(post => post.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(post => post.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<BlogPost>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(BlogPost post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BlogPost post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(BlogPost post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Posts.CountAsync();
    }
}
