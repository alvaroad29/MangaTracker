using MangaTracker.Api.Extensions;
using MangaTracker.Application.DTOs.Entries;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api/manga-list")]
[Authorize]
public class MangaListEntriesController(IUserMangaEntryService entryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add(TrackMangaRequest request)
        => Ok(await entryService.AddAsync(User.GetUserId(), request.MalId, request.Status));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ReadingStatus? status)
        => Ok(await entryService.GetAllAsync(User.GetUserId(), status));
    
    [HttpGet("by-manga/{malId:int}")]
    public async Task<IActionResult> GetByMalId(int malId)
    {
        var entry = await entryService.GetByMalIdAsync(User.GetUserId(), malId);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPut("{entryId:guid}")]
    public async Task<IActionResult> Update(Guid entryId, UpdateMangaEntryRequest request)
        => Ok(await entryService.UpdateAsync(User.GetUserId(), entryId, request));

    [HttpDelete("{entryId:guid}")]
    public async Task<IActionResult> Delete(Guid entryId)
    {
        await entryService.DeleteAsync(User.GetUserId(), entryId);
        return NoContent();
    }
}