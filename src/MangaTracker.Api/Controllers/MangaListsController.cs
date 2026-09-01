using MangaTracker.Api.Extensions;
using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize]
public class MangaListsController(IMangaListService listService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateMangaListRequest request)
        => Ok(await listService.CreateAsync(User.GetUserId(), request));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await listService.GetAllAsync(User.GetUserId()));

    [HttpPut("{listId:guid}")]
    public async Task<IActionResult> Update(Guid listId, UpdateMangaListRequest request)
        => Ok(await listService.UpdateAsync(User.GetUserId(), listId, request));

    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> Delete(Guid listId)
    {
        await listService.DeleteAsync(User.GetUserId(), listId);
        return NoContent();
    }

    [HttpGet("{listId:guid}/items")]
    public async Task<IActionResult> GetItems(Guid listId)
        => Ok(await listService.GetItemsAsync(User.GetUserId(), listId));

    [HttpPost("{listId:guid}/items/{entryId:guid}")]
    public async Task<IActionResult> AddItem(Guid listId, Guid entryId)
    {
        await listService.AddItemAsync(User.GetUserId(), listId, entryId);
        return NoContent();
    }

    [HttpDelete("{listId:guid}/items/{entryId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid listId, Guid entryId)
    {
        await listService.RemoveItemAsync(User.GetUserId(), listId, entryId);
        return NoContent();
    }
}