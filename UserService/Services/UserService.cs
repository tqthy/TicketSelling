using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Models;

namespace UserService.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    IMapper mapper)
    : IUserService
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager; // Often used for login, but we'll manually check password here for JWT generation
    private readonly IMapper _mapper = mapper;

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "User already exists with this email." };
        }

        var newUser = new ApplicationUser
        {
            UserName = registerDto.Username, // Or Email, depending on your preference
            Email = registerDto.Email,
            // EmailConfirmed = true // Or implement email confirmation flow
        };

        // Creates the user in the database
        var result = await userManager.CreateAsync(newUser, registerDto.Password);
        
        if (!result.Succeeded)
        {
            // Collect errors
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto { IsSuccess = false, Message = $"User registration failed: {errors}" };
        }
        

        await userManager.AddToRoleAsync(newUser, registerDto.Role.ToString()); // Add user to role
        
        return new AuthResponseDto { IsSuccess = true, Message = "User registered successfully.", UserId =  newUser.Id };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find the user by email
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
        }

        // Check the password
        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
        }

        // ** If password is valid, generate JWT token **
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found."));

        // Define token expiration (e.g., 1 hour)
        var tokenExpiry = DateTime.UtcNow.AddHours(1);
        
        // Prepare claims for the token payload
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (usually user ID)
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token identifier
        };
        
        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        // Create the security token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = tokenExpiry,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Create the token
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // Write the token string
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful.",
            Token = tokenString,
            Expiration = tokenDescriptor.Expires
        };
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            return await Task.FromResult(userManager.Users.Select(user => _mapper.Map<UserDto>(user)).ToList());
        }
        catch (Exception ex)
        {
            return [];
        }
    }

    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        // Find the user by ID
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return null; // User not found
        }
        // Map the updated properties
        _mapper.Map(updateUserDto, user);
        // Update the user in the database
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded) return _mapper.Map<UserDto>(user);
        // Handle errors
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new Exception($"User update failed: {errors}");
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
    // Find the user by ID
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return await Task.FromResult(false); // User not found
        }
        // Delete the user from the database
        var result = await userManager.DeleteAsync(user);
        return await Task.FromResult(result.Succeeded);
    }

    public Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        // Find the user by ID
        var user = userManager.Users.FirstOrDefault(u => u.Id == id.ToString());
        if (user == null)
        {
            return Task.FromResult<UserDto?>(null); // User not found
        }
        // Map the user to UserDto
        var userDto = _mapper.Map<UserDto>(user);
        return Task.FromResult(userDto)!;
    }
}