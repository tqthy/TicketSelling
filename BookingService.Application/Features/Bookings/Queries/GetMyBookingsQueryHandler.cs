using BookingService.Application.Features.Bookings.DTOs;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using MediatR;

namespace BookingService.Application.Features.Bookings.Queries;


public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<BookingDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetBookingsByUserIdAsync(request.UserId, cancellationToken);
        
        // TODO: Map the bookings to BookingDto
        return new List<BookingDto>();
        // return bookings.Select(booking => new BookingDto
        // {
        //     Id = booking.Id,
        //     EventId = booking.EventId,
        //     TotalPrice = booking.TotalPrice,
        //     Status = booking.Status,
        //     ExpiresAt = booking.ExpiresAt
        // }).ToList();
    }
}
