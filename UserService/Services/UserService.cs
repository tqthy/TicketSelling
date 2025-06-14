using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    IMapper mapper,
    ApplicationDbContext dbContext)
    : IUserService
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager; // Often used for login, but we'll manually check password here for JWT generation
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ApplicationDbContext _dbContext = dbContext;

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

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

        // Generate email confirmation token and link
        var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
        var confirmationLink = $"localhost:5106/confirm-email?userId={newUser.Id}&token={Uri.EscapeDataString(token)}";

        return new AuthResponseDto {
            IsSuccess = true,
            Message = "User registered successfully. Please check your email to confirm your account.",
            UserId = newUser.Id,
            EmailConfirmationLink = confirmationLink // For development/testing
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find the user by email
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
        }

        // Check the password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
        }

        // Generate JWT token (existing code)
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found."));
        var tokenExpiry = DateTime.UtcNow.AddHours(24);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = tokenExpiry,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Generate and save refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7), // 7 days expiry
            Created = DateTime.UtcNow
        };
        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful.",
            Token = tokenString,
            Expiration = tokenDescriptor.Expires,
            UserId = user.Id,
            RefreshToken = refreshToken
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

    public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var existingToken = _dbContext.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
        if (existingToken == null || existingToken.IsRevoked || existingToken.IsUsed || existingToken.Expires < DateTime.UtcNow)
        {
            return null;
        }

        // Mark the old refresh token as used
        existingToken.IsUsed = true;
        existingToken.IsRevoked = true;
        _dbContext.RefreshTokens.Update(existingToken);

        // Get the user
        var user = await _userManager.FindByIdAsync(existingToken.UserId);
        if (user == null)
        {
            return null;
        }

        // Generate new JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found."));
        var tokenExpiry = DateTime.UtcNow.AddHours(24);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = tokenExpiry,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Generate and save new refresh token
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        };
        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new RefreshTokenResponseDto
        {
            Token = tokenString,
            Expiration = tokenDescriptor.Expires ?? DateTime.UtcNow.AddHours(24),
            RefreshToken = newRefreshToken
        };
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string?> RequestPasswordResetAsync(PasswordResetRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return null;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"localhost:5106/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        return resetLink;
    }

    public async Task<bool> ResetPasswordAsync(PasswordResetDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return false;
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        return result.Succeeded;
    }
}