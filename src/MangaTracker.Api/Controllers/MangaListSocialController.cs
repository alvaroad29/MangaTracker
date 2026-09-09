using MangaTracker.Api.Extensions;
using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api")]
public class MangaListSocialController(IMangaListSocialService socialService) : ControllerBase
{
    [HttpGet("lists/public")]
    [AllowAnonymous] // explorar listas públicas no requiere estar logueado
    public async Task<IActionResult> SearchPublic([FromQuery] string? search, [FromQuery] int page = 1)
    {
        var currentUserId = TryGetCurrentUserId();
        return Ok(await socialService.SearchPublicListsAsync(currentUserId, search, page));
    }

    [HttpGet("users/{userId:guid}/lists")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserLists(Guid userId) // listas de un usuario en particular
    {
        var currentUserId = TryGetCurrentUserId(); // para saber si lo sigo o no a esa lista
        return Ok(await socialService.GetUserPublicListsAsync(userId, currentUserId));
    }

    [HttpGet("lists/followed")] // listas que sigue el usuario logueado
    [Authorize]
    public async Task<IActionResult> GetFollowed()
        => Ok(await socialService.GetFollowedListsAsync(User.GetUserId()));

    [HttpPost("lists/{listId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(Guid listId)
    {
        await socialService.FollowAsync(User.GetUserId(), listId);
        return NoContent();
    }

    [HttpDelete("lists/{listId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(Guid listId)
    {
        await socialService.UnfollowAsync(User.GetUserId(), listId);
        return NoContent();
    }
    
    [HttpGet("users/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserProfile(Guid userId)
        => Ok(await socialService.GetUserProfileAsync(userId));

    private Guid? TryGetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true) return null;
        return User.GetUserId();
    }
    
    
}