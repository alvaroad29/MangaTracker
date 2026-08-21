using MangaTracker.Application.DTOs.Auth;
using Riok.Mapperly.Abstractions;

namespace MangaTracker.Infrastructure.Identity;

[Mapper]
public partial class UserMapper
{
    [MapProperty(nameof(ApplicationUser.Id), nameof(UserDto.Id))]
    [MapProperty(nameof(ApplicationUser.Email), nameof(UserDto.Email))]
    public partial UserDto ToDto(ApplicationUser user);
}