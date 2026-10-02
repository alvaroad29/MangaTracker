using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using MangaTracker.Api.Extensions;
using MangaTracker.Application.DTOs.Backup;
using MangaTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaTracker.Api.Controllers;


[ApiController]
[Route("api/backup")]
[Authorize]
public class BackupController(IBackupService backupService) : ControllerBase
{
    [HttpGet("export/json")]
    public async Task<IActionResult> ExportJson()
    {
        var data = await backupService.BuildJsonExportAsync(User.GetUserId());
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        return File(Encoding.UTF8.GetBytes(json), "application/json", $"mangatracker-backup-{DateTime.UtcNow:yyyyMMdd}.json");
    }

    [HttpGet("export/mal-xml")]
    public async Task<IActionResult> ExportMalXml()
    {
        var xml = await backupService.BuildMalXmlExportAsync(User.GetUserId());
        return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"mangatracker-mal-export-{DateTime.UtcNow:yyyyMMdd}.xml");
    }

    [HttpPost("import/json")]
    public async Task<IActionResult> ImportJson(BackupExportDto backup)
        => Ok(await backupService.ImportJsonAsync(User.GetUserId(), backup));
}