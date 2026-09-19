using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.Interfaces;

public interface IMangaListService
{
    Task<MangaListDto> CreateAsync(Guid userId, CreateMangaListRequest request); 
    Task<IReadOnlyList<MangaListDto>> GetAllAsync(Guid userId); // listas de un user
    // Task<MangaListDto> GetByIdAsync(Guid? viewerUserId, Guid listId);
    Task<MangaListDto> UpdateAsync(Guid userId, Guid listId, UpdateMangaListRequest request); // act. info lista
    Task DeleteAsync(Guid userId, Guid listId);
    
    Task<MangaListDetailDto> GetDetailAsync(Guid? viewerUserId, Guid listId);

    // Task<IReadOnlyList<MangaListItemDto>> GetItemsAsync(Guid userId, Guid listId); // treaer items lista
    // Task<IReadOnlyList<MangaListItemDto>> GetItemsAsync(Guid? viewerUserId, Guid listId);
    Task AddItemAsync(Guid userId, Guid listId, Guid entryId); // agregar item lista (entry existente)
    Task RemoveItemAsync(Guid userId, Guid listId, Guid entryId);
    
    Task<MangaListItemDto> AddMangaByMalIdAsync(Guid userId, Guid listId, int malId, ReadingStatus defaultStatus);
}