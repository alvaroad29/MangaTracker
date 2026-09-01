using MangaTracker.Application.DTOs.Lists;

namespace MangaTracker.Application.Interfaces;

public interface IMangaListService
{
    Task<MangaListDto> CreateAsync(Guid userId, CreateMangaListRequest request); 
    Task<IReadOnlyList<MangaListDto>> GetAllAsync(Guid userId); // listas de un user
    Task<MangaListDto> UpdateAsync(Guid userId, Guid listId, UpdateMangaListRequest request); // act. info lista
    Task DeleteAsync(Guid userId, Guid listId);

    Task<IReadOnlyList<MangaListItemDto>> GetItemsAsync(Guid userId, Guid listId); // treaer items lista
    Task AddItemAsync(Guid userId, Guid listId, Guid entryId); // agregar item lista
    Task RemoveItemAsync(Guid userId, Guid listId, Guid entryId);
}