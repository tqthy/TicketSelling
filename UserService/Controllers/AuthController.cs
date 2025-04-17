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

        // Return 200 OK for registration, maybe with the success message
        // Avoid returning sensitive info like password hashes etc.
        return Ok(new { result.Message });
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
        return Ok(new { result.Token, result.Expiration });
    }
}