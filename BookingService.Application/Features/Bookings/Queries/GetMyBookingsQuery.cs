using BookingService.Application.Features.Bookings.DTOs;
using MediatR;

namespace BookingService.Application.Features.Bookings.Queries;

public class GetMyBookingsQuery : IRequest<List<BookingDto>>
{
    public Guid UserId { get; set; }
}