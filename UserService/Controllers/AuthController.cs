using System.Security.Claims;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace UserService.Controllers;

[Route("api/[controller]")] // Base route: api/auth
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    // Inject the service via the constructor
    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
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

    [HttpGet("google-login")]
    public IActionResult GoogleLogin(string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        _logger.LogInformation(redirectUrl);
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        //Check authentication response as mentioned on startup file as o.DefaultSignInScheme = "External"
        // var authenticateResult = await HttpContext.AuthenticateAsync("External");
        // if (!authenticateResult.Succeeded)
        //     return BadRequest(); // TODO: Handle this better.
        // //Check if the redirection has been done via google or any other links
        // if (authenticateResult.Principal.Identities.ToList()[0].AuthenticationType.ToLower() == "google")
        // {
        //     //check if principal value exists or not 
        //     if (authenticateResult.Principal != null)
        //     {
        //         //get google account id for any operation to be carried out on the basis of the id
        //         var googleAccountId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //         //claim value initialization as mentioned on the startup file with o.DefaultScheme = "Application"
        //         var claimsIdentity = new ClaimsIdentity("Application");
        //         if (authenticateResult.Principal != null)
        //         {
        //             //Now add the values on claim and redirect to the page to be accessed after successful login
        //             var details = authenticateResult.Principal.Claims.ToList();
        //             claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));// Full Name Of The User
        //             claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Email)); // Email Address of The User
        //             await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));
        //             return LocalRedirect(returnUrl);
        //         }
        //     }
        // }
        _logger.LogInformation("GoogleCallback endpoint hit");
        var authenticateResult = await HttpContext.AuthenticateAsync("External");
        if (!authenticateResult.Succeeded) return LocalRedirect("/error");
        // Log all claims for debugging
        var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
        var email = emailClaim?.Value;
        var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value
            ?? email?.Split('@')[0];
        var firstName = name?.Split(' ')[0] ?? string.Empty;
        var lastName = name?.Split(' ').Length > 1 ? name.Split(' ')[1] : string.Empty;
        // foreach (var claim in User.Claims)
        // {
        //     _logger.LogInformation($"Claim type: {claim.Type}, value: {claim.Value}");
        // }
        // var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value
        //     ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        // var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? email?.Split('@')[0];
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { Message = "Google authentication failed: email not found." });
        }
        
        // Check if user exists, if not, register
        var result = await _userService.RegisterWithGoogleAsync(email, firstName, lastName);
        if (!result.IsSuccess && result.Message != "User already exists with this email.")
        {
            return BadRequest(new { result.Message });
        }
        
        // If user already exists, just log them in (generate JWT)
        if (!result.IsSuccess && result.Message == "User already exists with this email.")
        {
            // You may want to implement a LoginWithGoogleAsync, but for now, just generate a JWT
            // Find user and generate JWT
            // ...
            // For simplicity, redirect to returnUrl
            return LocalRedirect(returnUrl);
        }

        // On success, redirect or return token
        // For now, just redirect
        return LocalRedirect(returnUrl);
    }

    // [HttpPost("logout")] // Route: POST api/auth/logout
    // public IActionResult Logout()
    // {
    //     // For JWT, logout is typically handled client-side by removing the token.
    //     // If you have server-side session management, you can clear the session here.
    //     return Ok(new { Message = "Logged out successfully." });
    // }
}