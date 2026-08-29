using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;

[ApiController]
[Route("api/manga")]
public class MangaController(IMangaCatalogService catalogService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { title = "El parámetro 'q' es obligatorio." });

        var result = await catalogService.SearchMangaAsync(q, page);
        return Ok(result);
    }

    [HttpGet("{malId:int}")]
    public async Task<IActionResult> GetByMalId(int malId)
    {
        var manga = await catalogService.GetMangaByMalIdAsync(malId);
        return manga is null ? NotFound() : Ok(manga);
    }
    
    [HttpPost("{malId:int}/snapshot")]
    [Authorize] // requiere estar logueado — no tiene sentido crear snapshots sin usuario detrás
    public async Task<IActionResult> GetOrCreateSnapshot(int malId, [FromServices] IMangaSnapshotService snapshotService)
    {
        var snapshot = await snapshotService.GetOrCreateAsync(malId);
        return Ok(snapshot);
    }
}