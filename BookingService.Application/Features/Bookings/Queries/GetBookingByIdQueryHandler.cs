using BookingService.Application.Features.Bookings.DTOs;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using MediatR;

namespace BookingService.Application.Features.Bookings.Queries;

public class GetBookingByIdResult : Result
{
    public BookingDto? Booking { get; set; }
}

public class GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingByIdQuery, GetBookingByIdResult>
{
    public async Task<GetBookingByIdResult> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken = default)
    {
        var result = await bookingRepository.FindByIdAsync(request.BookingId, false);
        if (result == null)
        {
            return new GetBookingByIdResult { Booking = null, Success = false, Message = "Booking not found." };
        }
        if (result.UserId != request.UserId)
        {
            return new GetBookingByIdResult { Success = false, Message = "Unauthorized." };
        }
        var bookingDto = new BookingDto
        {
            Id = result.Id.ToString(),
            EventId = result.EventId.ToString(),
            TotalPrice = result.TotalPrice,
            Status = result.Status,
            ExpiresAt = result.ExpiresAt
        };
        return new GetBookingByIdResult
        {
            Booking = bookingDto,
            Success = true,
            Message = "Booking retrieved successfully."
        };
    }
}