using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Lists;

public record AddMangaToListRequest(int MalId, ReadingStatus DefaultStatus);