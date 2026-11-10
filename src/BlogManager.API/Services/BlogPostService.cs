using System.Text.RegularExpressions;
using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using BlogManager.API.Models;
using BlogManager.API.Repositories;

namespace BlogManager.API.Services;

public class BlogPostService : IBlogPostService
{
    private readonly IBlogPostRepository _repository;

    public BlogPostService(IBlogPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PostResponseDto>> GetPostsAsync(int page, int pageSize, string? search, PostStatus? status)
    {
        var pagedPosts = await _repository.GetPagedAsync(page, pageSize, search, status);

        return new PagedResult<PostResponseDto>
        {
            Items = pagedPosts.Items.Select(MapToResponse).ToList(),
            Page = pagedPosts.Page,
            PageSize = pagedPosts.PageSize,
            TotalCount = pagedPosts.TotalCount
        };
    }

    public async Task<PostResponseDto?> GetByIdAsync(int id)
    {
        var post = await _repository.GetByIdAsync(id);
        return post is null ? null : MapToResponse(post);
    }

    public async Task<PostResponseDto> CreateAsync(CreatePostDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Content is required.");
        }

        var generatedSlug = GenerateSlug(dto.Title);
        var existingBySlug = await _repository.GetBySlugAsync(generatedSlug);
        if (existingBySlug is not null)
        {
            throw new InvalidOperationException("A post with this slug already exists.");
        }

        var post = new BlogPost
        {
            Title = dto.Title.Trim(),
            Slug = generatedSlug,
            Content = dto.Content.Trim(),
            Excerpt = string.IsNullOrWhiteSpace(dto.Excerpt) ? null : dto.Excerpt.Trim(),
            FeaturedImageUrl = dto.FeaturedImageUrl,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _repository.AddAsync(post);
        return MapToResponse(post);
    }

    public async Task<PostResponseDto?> UpdateAsync(int id, UpdatePostDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            existing.Title = dto.Title.Trim();
            existing.Slug = await GenerateUniqueSlugAsync(existing.Title, id);
        }

        if (!string.IsNullOrWhiteSpace(dto.Content))
        {
            existing.Content = dto.Content.Trim();
        }

        if (dto.Excerpt != null)
        {
            existing.Excerpt = string.IsNullOrWhiteSpace(dto.Excerpt) ? null : dto.Excerpt.Trim();
        }

        if (dto.FeaturedImageUrl != null)
        {
            existing.FeaturedImageUrl = dto.FeaturedImageUrl;
        }

        if (dto.Status.HasValue)
        {
            existing.Status = dto.Status.Value;
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(existing);
        return MapToResponse(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        await _repository.DeleteAsync(existing);
        return true;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync()
    {
        var posts = await _repository.GetPagedAsync(1, int.MaxValue, null, null);
        var postList = posts.Items;

        return new DashboardSummaryDto
        {
            TotalPosts = postList.Count,
            PublishedPosts = postList.Count(post => post.Status == PostStatus.Published),
            DraftPosts = postList.Count(post => post.Status == PostStatus.Draft),
            RecentPosts = postList
                .OrderByDescending(post => post.CreatedAt)
                .Take(5)
                .Select(MapToResponse)
                .ToList()
        };
    }

    private static PostResponseDto MapToResponse(BlogPost post)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Excerpt = post.Excerpt,
            FeaturedImageUrl = post.FeaturedImageUrl,
            Status = post.Status,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? currentPostId = null)
    {
        var baseSlug = GenerateSlug(title);
        var slug = baseSlug;
        var suffix = 2;

        while (true)
        {
            var existing = await _repository.GetBySlugAsync(slug);
            if (existing is null || (currentPostId.HasValue && existing.Id == currentPostId.Value))
            {
                return slug;
            }

            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.Trim();
        slug = slug.ToLowerInvariant();
        slug = slug.Replace("&", "and");
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "untitled-post" : slug;
    }
}
