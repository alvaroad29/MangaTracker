using MangaTracker.Application.DTOs.Entries;
using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.Interfaces;

public interface IUserMangaEntryService
{
    Task<MangaEntryDto> AddAsync(Guid userId, int malId);
    Task<IReadOnlyList<MangaEntryDto>> GetAllAsync(Guid userId, ReadingStatus? statusFilter);
    Task<MangaEntryDto> UpdateAsync(Guid userId, Guid entryId, UpdateMangaEntryRequest request);
    Task DeleteAsync(Guid userId, Guid entryId);
    Task<MangaEntryDto?> GetByMalIdAsync(Guid userId, int malId);
}