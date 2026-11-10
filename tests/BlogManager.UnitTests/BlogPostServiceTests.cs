using BlogManager.API.Data;
using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using BlogManager.API.Models;
using BlogManager.API.Repositories;
using BlogManager.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogManager.UnitTests;

public class BlogPostServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateSlugAndPersistPost()
    {
        await using var context = CreateContext();
        var repository = new BlogPostRepository(context);
        var service = new BlogPostService(repository);

        var dto = new CreatePostDto
        {
            Title = "My First Blog Post",
            Content = "This is the content.",
            Excerpt = "Short excerpt",
            Status = PostStatus.Draft,
            FeaturedImageUrl = "https://example.com/image.jpg"
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal("my-first-blog-post", result.Slug);
        Assert.Equal(PostStatus.Draft, result.Status);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForDuplicateSlug()
    {
        await using var context = CreateContext();
        var repository = new BlogPostRepository(context);
        var service = new BlogPostService(repository);

        await service.CreateAsync(new CreatePostDto { Title = "Same Title", Content = "One" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreatePostDto { Title = "Same Title", Content = "Two" }));

        Assert.Contains("slug", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFieldsAndTimestamp()
    {
        await using var context = CreateContext();
        var repository = new BlogPostRepository(context);
        var service = new BlogPostService(repository);

        var created = await service.CreateAsync(new CreatePostDto { Title = "Original Title", Content = "Body" });

        var updated = await service.UpdateAsync(created.Id, new UpdatePostDto
        {
            Title = "Updated Title",
            Excerpt = "Updated excerpt",
            Status = PostStatus.Published,
            Content = "New content"
        });

        Assert.NotNull(updated);
        Assert.Equal("updated-title", updated.Slug);
        Assert.Equal(PostStatus.Published, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldFilterAndPaginateResults()
    {
        await using var context = CreateContext();
        var repository = new BlogPostRepository(context);
        var service = new BlogPostService(repository);

        await service.CreateAsync(new CreatePostDto { Title = "Technology Post", Content = "Tech content", Status = PostStatus.Published });
        await service.CreateAsync(new CreatePostDto { Title = "Finance Post", Content = "Finance content", Status = PostStatus.Draft });
        await service.CreateAsync(new CreatePostDto { Title = "Another Technology Post", Content = "More tech", Status = PostStatus.Published });

        var paged = await service.GetPostsAsync(1, 2, "technology", PostStatus.Published);

        Assert.Equal(2, paged.Items.Count);
        Assert.Equal(2, paged.TotalCount);
        Assert.All(paged.Items, item => Assert.Contains("technology", item.Title, StringComparison.OrdinalIgnoreCase));
    }
}
