using BlogManager.API.DTOs;
using BlogManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!_authService.ValidateCredentials(request.Username, request.Password))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = _authService.CreateToken(request.Username);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = request.Username
        });
    }
}
