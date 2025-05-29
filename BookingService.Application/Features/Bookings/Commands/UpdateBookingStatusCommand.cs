using BookingService.Application.Features.Bookings.DTOs;
using MediatR;

namespace BookingService.Application.Features.Bookings.Commands;

public class UpdateBookingStatusCommand : IRequest<UpdateBookingStatusResponse>
{
    public Guid BookingId { get; set; }
    public string Status { get; set; } = string.Empty; // Default to empty string if not provided
}