// BookingService.Api/Controllers/BookingsController.cs

using MediatR; // For ISender
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingService.Api.DTOs;
// Reference DTOs/Commands/Queries from Application or Common layer
using BookingService.Application.Features.Bookings.Commands;
using BookingService.Application.Features.Bookings.Queries;
using BookingService.Application.Features.Bookings.DTOs;
using BookingService.Application.Features.Events.Queries; // For API Request DTO if needed

namespace BookingService.Api.Controllers;

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
    [Authorize(Roles = "User")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult>
        CreateBooking([FromBody] CreateBookingRequestDto apiRequest) // Use API-specific DTO if needed
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Invalid user identifier." });
        }
        
        var ipAddress = "127.0.0.1";
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        
        
        // if (remoteIp != null)
        // {
        //     if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        //     {
        //         remoteIp = Dns.GetHostEntry(remoteIp).AddressList.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        //     }
        //
        //     ipAddress = remoteIp != null ? remoteIp.ToString() : "127.0.0.1";
        // }
        
        // Map API request DTO to Application Command
        var command = new CreateBookingCommand
        {
            EventId = apiRequest.EventId,
            SeatIds = apiRequest.SeatIds,
            UserId = userId,
            UserIpAddress = ipAddress 
        };

        _logger.LogInformation("Dispatching CreateBookingCommand for User {UserId}", userId);
        var result = await _mediator.Send(command);

        // Handle result from Application layer
        if (result.Success && result.BookingDetails != null)
        {
            return CreatedAtAction(nameof(GetBookingById), new { bookingId = result.BookingDetails.BookingId },
                result.BookingDetails);
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
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = result.ErrorMessage ?? "An error occurred while processing the booking." });
        }
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "User")]
    [ProducesResponseType(typeof(List<BookingDto>), StatusCodes.Status200OK)]
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
    [HttpGet("{bookingId}")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookingById(Guid bookingId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Invalid user identifier." });
        }

        var query = new GetBookingByIdQuery { BookingId = bookingId, UserId = userId };
        _logger.LogInformation("Dispatching GetBookingByIdQuery for Booking {BookingId} and User {UserId}", bookingId,
            userId);

        var result = await _mediator.Send(query);
        if (result.Success) return Ok(result.Booking);
        if (result.ErrorMessage?.Contains("Unauthorized") ?? false)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        if (result.ErrorMessage?.Contains("not found") ?? false)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.LogError("Booking command failed: {ErrorMessage}", result.ErrorMessage);
        return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.ErrorMessage });
    }
    
    [HttpGet("status/{eventId}")]
    [Authorize(Roles = "User,Admin,Organizer")]
    public async Task<IActionResult> GetBookingStatus(Guid eventId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Invalid user identifier." });
        }

        var query = new GetEventSeatsQuery { EventId = eventId };
        _logger.LogInformation("Dispatching GetEventSeatsQuery for Event {EventId} and User {UserId}", eventId, userId);
        var result = await _mediator.Send(query);
        if (result.Count == 0)
        {
            return NotFound(new { message = "No seats found for the specified event." });
        }
        return Ok(result);
    }
}

