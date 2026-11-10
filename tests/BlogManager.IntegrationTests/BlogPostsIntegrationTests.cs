using System.Net;
using System.Net.Http.Json;
using BlogManager.API.Data;
using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogManager.IntegrationTests;

public class BlogPostsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BlogPostsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPosts_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/posts?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnCreated()
    {
        var dto = new CreatePostDto
        {
            Title = "Integration Post",
            Content = "This is a test post.",
            Status = PostStatus.Published,
            Excerpt = "Integration"
        };

        var response = await _client.PostAsJsonAsync("/api/posts", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetPost_WhenNotFound_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/posts/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithDuplicateSlug_ShouldReturnConflict()
    {
        var first = new CreatePostDto { Title = "Duplicate Title", Content = "First body" };
        var second = new CreatePostDto { Title = "Duplicate Title", Content = "Second body" };

        var firstResponse = await _client.PostAsJsonAsync("/api/posts", first);
        var secondResponse = await _client.PostAsJsonAsync("/api/posts", second);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("BlogManagerIntegrationTests"));
        });
    }
}
