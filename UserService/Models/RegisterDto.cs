using System.ComponentModel.DataAnnotations;
using UserService.Data.Enum;

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
    
    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.User; 
}