using BlogManager.API.DTOs;
using BlogManager.API.Enums;
using BlogManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogManager.API.Controllers;

[ApiController]
[Route("api/posts")]
public class BlogPostsController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;

    public BlogPostsController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostResponseDto>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] PostStatus? status = null)
    {
        var posts = await _blogPostService.GetPostsAsync(page, pageSize, search, status);
        return Ok(posts);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
    {
        var summary = await _blogPostService.GetDashboardAsync();
        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> GetPost(int id)
    {
        var post = await _blogPostService.GetByIdAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var createdPost = await _blogPostService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> UpdatePost(int id, [FromBody] UpdatePostDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updatedPost = await _blogPostService.UpdateAsync(id, dto);
        if (updatedPost is null)
        {
            return NotFound();
        }

        return Ok(updatedPost);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var deleted = await _blogPostService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
