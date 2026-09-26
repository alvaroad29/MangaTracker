using MangaTracker.Application.DTOs.Auth;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MangaTracker.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    UserMapper userMapper) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ConflictAppException("Ya existe una cuenta con ese email.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new AppValidationException(result.Errors.Select(e => e.Description));

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            throw new UnauthorizedAppException();

        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            throw new UnauthorizedAppException();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var result = await refreshTokenService.ValidateAndRotateAsync(refreshToken);
        if (!result.Success)
            throw new UnauthorizedAppException(result.FailureReason ?? "Refresh token inválido.");

        var user = await userManager.FindByIdAsync(result.UserId.ToString())
            ?? throw new UnauthorizedAppException();

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user.Id, user.Email!);

        return new AuthResponse(accessToken, result.NewPlainToken!, expiresAt, userMapper.ToDto(user));
    }

    public Task LogoutAsync(string refreshToken) => refreshTokenService.RevokeAsync(refreshToken);
    
    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
                   ?? throw new UnauthorizedAppException("El usuario ya no existe.");

        return userMapper.ToDto(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user.Id, user.Email!);
        var refreshToken = await refreshTokenService.GenerateAsync(user.Id);
        return new AuthResponse(accessToken, refreshToken, expiresAt, userMapper.ToDto(user));
    }
}