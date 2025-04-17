using Microsoft.AspNetCore.Identity;
using UserService.Data.Enum;


namespace UserService.Data;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User; // Default role is User
}