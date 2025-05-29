using BookingService.Application.Features.Bookings.DTOs;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings.Commands;

public class UpdateBookingStatusCommandHandler(
    IBookingRepository bookingRepository,
    ILogger<UpdateBookingStatusCommandHandler> logger,
    IEventSeatStatusRepository seatStatusRepository)
    : IRequestHandler<UpdateBookingStatusCommand, UpdateBookingStatusResponse>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly ILogger<UpdateBookingStatusCommandHandler> _logger = logger;
    private readonly IEventSeatStatusRepository _seatStatusRepository = seatStatusRepository;


    public async Task<UpdateBookingStatusResponse> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.BookingId == Guid.Empty)
        {
            _logger.LogError("Invalid booking ID provided.");
            throw new ArgumentException("Booking ID cannot be empty.", nameof(request.BookingId));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogError("Invalid booking status provided.");
            throw new ArgumentException("Booking status cannot be empty.", nameof(request.Status));
        }

        var booking = await _bookingRepository.FindByIdAsync(request.BookingId, true);
        if (booking == null)
        {
            _logger.LogError($"Booking with ID {request.BookingId} not found.");
            throw new KeyNotFoundException($"Booking with ID {request.BookingId} not found.");
        }

        booking.UpdateStatus(request.Status);

        _bookingRepository.Update(booking);
        var seatIds = booking.BookedSeats.Select(bs => bs.SeatId).ToList();
        var newSeatStatus = request.Status switch
        {
            BookingStatus.Confirmed => SeatAvailabilityStatus.Sold,
            BookingStatus.Failed => SeatAvailabilityStatus.Available,
            _ => throw new InvalidOperationException($"Cannot update seat status for booking with status '{request.Status}'.")
        };
        await _seatStatusRepository.UpdateSeatStatuses(booking.Id, seatIds, booking.EventId, newSeatStatus);

        return new UpdateBookingStatusResponse
        {
            Status = request.Status,
            Success = true,
            Message = $"Booking status updated to '{request.Status}' for booking ID {request.BookingId}."
        };
        
        
    }
}