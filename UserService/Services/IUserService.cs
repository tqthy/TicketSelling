using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<string?> RequestPasswordResetAsync(PasswordResetRequestDto dto);
    Task<bool> ResetPasswordAsync(PasswordResetDto dto);
}