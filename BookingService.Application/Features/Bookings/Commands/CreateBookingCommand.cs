using MediatR;

namespace BookingService.Application.Features.Bookings.Commands;

public class CreateBookingCommand : IRequest<BookingResult> // Use a result object
{
    public Guid EventId { get; set; }
    public Guid SeatId { get; set; }
    public Guid UserId { get; set; } // Passed securely from API layer
    public string UserIpAddress { get; set; } = "127.0.0.1"; // Passed securely from API layer
}