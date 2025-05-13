using BookingService.Application.Features.Bookings.DTOs;
using MediatR;

namespace BookingService.Application.Features.Bookings.Queries;

public class GetBookingByIdQuery : IRequest<GetBookingByIdResult>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
}