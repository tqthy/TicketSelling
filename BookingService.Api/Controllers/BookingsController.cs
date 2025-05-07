// BookingService.Api/Controllers/BookingsController.cs
using MediatR; // For ISender
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
// Reference DTOs/Commands/Queries from Application or Common layer
using BookingService.Application.Features.Bookings.Commands;
using BookingService.Application.Features.Bookings.Queries;
using BookingService.Application.Features.Bookings.DTOs; // For API Request DTO if needed

[ApiController]
[Route("api/[controller]")]
[Authorize] // Applied at controller level
public class BookingsController : ControllerBase
{
    private readonly ISender _mediator; 
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(ISender mediator, ILogger<BookingsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingApiRequest apiRequest) // Use API-specific DTO if needed
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Invalid user identifier." });
        }

        // Map API request DTO to Application Command
        var command = new CreateBookingCommand
        {
            EventId = apiRequest.EventId,
            SeatId = apiRequest.SeatId,
            UserId = userId
        };

        _logger.LogInformation("Dispatching CreateBookingCommand for User {UserId}", userId);
        var result = await _mediator.Send(command);

        // Handle result from Application layer
        if (result.Success && result.BookingDetails != null)
        {
            // Use nameof(GetBookingById) assuming you'll create that query/endpoint
            return CreatedAtAction("GetBookingById_Placeholder", new { bookingId = result.BookingDetails.BookingId }, result.BookingDetails);
        }
        else if (result.IsConflict)
        {
            return Conflict(new { message = result.ErrorMessage ?? "Seat conflict." });
        }
        else if (result.ErrorMessage?.Contains("not found") ?? false) // Hint based on error message
        {
             return NotFound(new { message = result.ErrorMessage });
        }
        else if (result.ErrorMessage?.Contains("pricing information") ?? false) // Hint based on error message
        {
             return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = result.ErrorMessage });
        }
        else // Generic internal error or bad request based on context
        {
             // Log the specific error message if available
             _logger.LogError("Booking command failed: {ErrorMessage}", result.ErrorMessage);
             return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.ErrorMessage ?? "An error occurred while processing the booking."});
        }
    }

    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(List<BookingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
         if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Invalid user identifier." });
        }

        var query = new GetMyBookingsQuery { UserId = userId };
        _logger.LogInformation("Dispatching GetMyBookingsQuery for User {UserId}", userId);

        var result = await _mediator.Send(query); // Result is List<BookingDetailDto>

        return Ok(result);
    }

     // Placeholder for GET by ID route name used in CreatedAtAction
    [HttpGet("{bookingId}", Name = "GetBookingById_Placeholder")]
    [AllowAnonymous] // Or Authorize if needed
     public IActionResult GetBookingById_Placeholder(Guid bookingId)
    {
        // Implementation using a GetBookingByIdQuery would go here
        return Ok($"Placeholder for booking {bookingId}");
    }
}

// Define API-specific request DTO if different from Application layer Command
public class CreateBookingApiRequest
{
    [Required]
    public Guid EventId { get; set; }
    [Required]
    public Guid SeatId { get; set; }
}