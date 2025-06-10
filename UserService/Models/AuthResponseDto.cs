namespace UserService.Models;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; } // The JWT
    public DateTime? Expiration { get; set; } // When the token expires
    public string UserId { get; set; } = string.Empty; // The ID of the user
    public string? RefreshToken { get; set; } // The refresh token
    public string? EmailConfirmationLink { get; set; } // For development/testing
}