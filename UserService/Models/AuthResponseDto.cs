namespace UserService.Models;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; } // The JWT
    public DateTime? Expiration { get; set; } // When the token expires
}