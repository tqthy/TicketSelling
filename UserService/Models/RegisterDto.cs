using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty; 

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}