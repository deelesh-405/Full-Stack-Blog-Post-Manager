using System.Net;
using System.Net.Http.Headers;
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
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/posts?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPosts_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/posts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_PreflightAllowsConfiguredFrontendOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task CreatePost_ShouldReturnCreated()
    {
        await AuthenticateAsync();

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
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/posts/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithDuplicateSlug_ShouldReturnConflict()
    {
        await AuthenticateAsync();

        var first = new CreatePostDto { Title = "Duplicate Title", Content = "First body" };
        var second = new CreatePostDto { Title = "Duplicate Title", Content = "Second body" };

        var firstResponse = await _client.PostAsJsonAsync("/api/posts", first);
        var secondResponse = await _client.PostAsJsonAsync("/api/posts", second);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    private async Task AuthenticateAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "admin",
            Password = "Password123!"
        });

        login.EnsureSuccessStatusCode();

        var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Auth__Username", "admin");
        Environment.SetEnvironmentVariable("Auth__Password", "Password123!");
        Environment.SetEnvironmentVariable("Jwt__Key", "IntegrationTestsNeedA32ByteSigningKey");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "Fieldnotes");
        Environment.SetEnvironmentVariable("Jwt__Audience", "FieldnotesClient");
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost:5173");
    }

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
