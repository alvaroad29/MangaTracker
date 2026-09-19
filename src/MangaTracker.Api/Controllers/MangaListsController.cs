using MangaTracker.Api.Extensions;
using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize] // controller para manejar mis listas (usuario que esta logueado)
public class MangaListsController(IMangaListService listService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateMangaListRequest request)
        => Ok(await listService.CreateAsync(User.GetUserId(), request));

    [HttpGet]
    public async Task<IActionResult> GetAll() // obtener listas de un usuario
        => Ok(await listService.GetAllAsync(User.GetUserId()));
   
    [HttpGet("{listId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetail(Guid listId)
    {
        var viewerId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        return Ok(await listService.GetDetailAsync(viewerId, listId));
    }
        
    // [HttpGet("{listId:guid}")]
    // [AllowAnonymous]
    // public async Task<IActionResult> GetById(Guid listId)
    // {
    //     var viewerId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
    //     return Ok(await listService.GetByIdAsync(viewerId, listId));
    // }

    [HttpPut("{listId:guid}")]
    public async Task<IActionResult> Update(Guid listId, UpdateMangaListRequest request)
        => Ok(await listService.UpdateAsync(User.GetUserId(), listId, request));

    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> Delete(Guid listId)
    {
        await listService.DeleteAsync(User.GetUserId(), listId);
        return NoContent();
    }

    // [HttpGet("{listId:guid}/items")]
    // [AllowAnonymous]
    // public async Task<IActionResult> GetItems(Guid listId)
    // {
    //     var viewerId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
    //     return Ok(await listService.GetItemsAsync(viewerId, listId));
    // }
    
    [HttpPost("{listId:guid}/items/by-manga")]
    public async Task<IActionResult> AddMangaByMalId(Guid listId, AddMangaToListRequest request)
        => Ok(await listService.AddMangaByMalIdAsync(User.GetUserId(), listId, request.MalId, request.DefaultStatus));

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