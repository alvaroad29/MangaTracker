using MangaTracker.Api.Extensions;
using MangaTracker.Application.DTOs.Auth;
using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        => Ok(await authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        => Ok(await authService.LoginAsync(request));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] string refreshToken)
        => Ok(await authService.RefreshAsync(refreshToken));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await authService.LogoutAsync(refreshToken);
        return NoContent();
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
        => Ok(await authService.GetCurrentUserAsync(User.GetUserId()));
}