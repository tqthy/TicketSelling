using BookingService.Application.Features.Bookings.DTOs;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using MediatR;

namespace BookingService.Application.Features.Bookings.Queries;


public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<GetMyBookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<GetMyBookingResponse>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetBookingsByUserIdAsync(request.UserId, cancellationToken);
        // TODO: Map the bookings to BookingDto
        var result = bookings.Select(booking => new GetMyBookingResponse
        {
            Id = booking.Id.ToString(),
            EventId = booking.EventId.ToString(),
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            UserId = booking.UserId.ToString(),
            ExpiresAt = booking.ExpiresAt,
            BookedSeats = booking.BookedSeats.Select(seat => new BookedSeatDetailDto
            {
                SeatId = seat.SeatId.ToString(),
                PriceAtBooking =  seat.PriceAtBooking,
                
            }).ToList()
        }).ToList();

        return result;
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
