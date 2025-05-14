using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/[controller]")] // Base route: api/user
public class UserController(IUserService userService, ILogger<UserController> logger, IMapper mapper)
    : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    
    // Get all users
    // GET: api/user
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users); 
    }
    
    // Get user by ID
    // GET: api/user/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user); 
    }
    
    // Update user
    // Patch: api/user/{id}
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedUser = await userService.UpdateUserAsync(id, updateUserDto);
            if (updatedUser == null)
            {
                return NotFound();
            }

            return Ok(updatedUser);   
        } catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user.");
        }
    }
    
    // Delete user
    // DELETE: api/user/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        var result = await userService.DeleteUserAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent(); // 204 No Content
    }
    
}