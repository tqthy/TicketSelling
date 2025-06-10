using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/[controller]")] // Base route: api/auth
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    // Inject the service via the constructor
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")] // Route: POST api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid) // Basic validation based on DTO attributes
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.RegisterAsync(registerDto);

        if (!result.IsSuccess)
        {
            return BadRequest(new { result.Message }); // Return error message
        }

        // Return 201 created for registration, maybe with the success message
        return CreatedAtAction(
            "GetUserByIdAsync",
            "User",
            new { id = result.UserId },
            new { result.Message, result.EmailConfirmationLink }
        );
    }

    [HttpPost("login")] // Route: POST api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.LoginAsync(loginDto);

        if (!result.IsSuccess)
        {
            // Use Unauthorized for login failures generally
            return Unauthorized(new { result.Message });
        }

        // Return the token upon successful login
        return Ok(new { result.Token, result.Expiration, result.RefreshToken });
    }

    [HttpPost("refresh-token")] // Route: POST api/auth/refresh-token
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _userService.RefreshTokenAsync(request);
        if (result == null)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token." });
        }
        return Ok(result);
    }

    [HttpGet("confirm-email")] // Route: GET api/auth/confirm-email
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest(new { Message = "UserId and token are required." });
        }
        var result = await _userService.ConfirmEmailAsync(userId, token);
        if (result)
        {
            return Ok(new { Message = "Email confirmed successfully." });
        }
        return BadRequest(new { Message = "Invalid or expired confirmation link." });
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var resetLink = await _userService.RequestPasswordResetAsync(dto);
        if (resetLink == null)
            return NotFound(new { Message = "User with this email does not exist." });
        
        return Ok(new { Message = "Password reset link generated.", ResetLink = resetLink });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _userService.ResetPasswordAsync(dto);
        if (!result)
            return BadRequest(new { Message = "Invalid token or user ID, or password does not meet requirements." });
        return Ok(new { Message = "Password has been reset successfully." });
    }

    // [HttpPost("logout")] // Route: POST api/auth/logout
    // public IActionResult Logout()
    // {
    //     // For JWT, logout is typically handled client-side by removing the token.
    //     // If you have server-side session management, you can clear the session here.
    //     return Ok(new { Message = "Logged out successfully." });
    // }
}