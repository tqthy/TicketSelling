// BookingService.Application/IntegrationEventHandlers/PaymentSucceededIntegrationEventHandler.cs

using MassTransit;
using Common.Messages; // Assuming PaymentSucceeded is here
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data; // For BookingDbContext
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For transaction
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Application.IntegrationEventHandlers
{
    public class PaymentSucceededIntegrationEventHandler : IConsumer<PaymentSucceeded>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventSeatStatusRepository _seatStatusRepository;
        private readonly BookingDbContext _dbContext;
        private readonly ILogger<PaymentSucceededIntegrationEventHandler> _logger;

        public PaymentSucceededIntegrationEventHandler(
            IBookingRepository bookingRepository,
            IEventSeatStatusRepository seatStatusRepository,
            BookingDbContext dbContext,
            ILogger<PaymentSucceededIntegrationEventHandler> logger)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _seatStatusRepository =
                seatStatusRepository ?? throw new ArgumentNullException(nameof(seatStatusRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<PaymentSucceeded> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received PaymentSucceeded event for BookingId: {BookingId}", message.BookingId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

            try
            {
                var booking = await _bookingRepository.FindByIdAsync(message.BookingId, includeSeats: true);

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking {BookingId} not found when processing PaymentSucceeded event. Ignoring message.",
                        message.BookingId);
                    await transaction.RollbackAsync(context.CancellationToken);
                    return;
                }

                // Idempotency check / State validation
                if (booking.Status == BookingStatus.Confirmed)
                {
                    _logger.LogInformation(
                        "Booking {BookingId} is already confirmed. Ignoring duplicate PaymentSucceeded event.",
                        message.BookingId);
                    await transaction.RollbackAsync(context.CancellationToken);
                    return;
                }

                if (booking.Status != BookingStatus.PendingPayment)
                {
                    _logger.LogWarning(
                        "Received PaymentSucceeded for Booking {BookingId} which has status {Status} (expected PendingPayment). Processing cannot continue reliably. Rolling back.",
                        message.BookingId, booking.Status);
                    await transaction.RollbackAsync(context.CancellationToken);
                    // Maybe throw or move to error queue? This indicates a potentially inconsistent state.
                    return;
                }

                var bookedSeat = booking.BookedSeats.FirstOrDefault();
                if (bookedSeat == null)
                {
                    _logger.LogError(
                        "Inconsistent state: Booking {BookingId} has no BookedSeat associated during PaymentSucceeded processing. Rolling back.",
                        message.BookingId);
                    await transaction.RollbackAsync(context.CancellationToken);
                    throw new InvalidOperationException($"Booking {message.BookingId} has no associated seat.");
                }

                var seatStatus =
                    await _seatStatusRepository.FindByEventAndSeatAsync(booking.EventId, bookedSeat.SeatId);

                if (seatStatus == null)
                {
                    _logger.LogError(
                        "Inconsistent state: EventSeatStatus not found for Event {EventId}, Seat {SeatId} associated with confirmed Booking {BookingId}. Rolling back.",
                        booking.EventId, bookedSeat.SeatId, message.BookingId);
                    await transaction.RollbackAsync(context.CancellationToken);
                    throw new InvalidOperationException($"EventSeatStatus not found for booking {message.BookingId}.");
                }


                // --- Apply Domain Logic ---
                _logger.LogInformation("Updating Booking {BookingId} status to Confirmed.", message.BookingId);
                booking.ConfirmPayment(); // Update booking aggregate

                bool seatSoldSuccessfully = false;
                // Check if seat is still reserved by this booking before marking as sold
                if (seatStatus.Status == SeatAvailabilityStatus.Reserved && seatStatus.CurrentBookingId == booking.Id)
                {
                    try
                    {
                        _logger.LogInformation("Marking Seat {SeatId} as Sold for Booking {BookingId}.",
                            seatStatus.SeatId, booking.Id);
                        seatStatus.Sell(booking.Id); // Use domain method
                        seatSoldSuccessfully = true;
                    }
                    catch (InvalidOperationException
                           ex) // Should ideally not happen if the status check above passed, but belt-and-suspenders
                    {
                        _logger.LogError(ex,
                            "Failed to mark Seat {SeatId} as Sold for Booking {BookingId} even though it appeared reserved. Status: {SeatStatus}",
                            seatStatus.SeatId, booking.Id, seatStatus.Status);
                    }
                }
                else
                {
                    // This is the race condition / timeout case. The reservation expired or was taken by someone else
                    // *just* before the payment confirmation arrived.
                    _logger.LogWarning(
                        "Seat {SeatId} for Event {EventId} was not in the expected 'Reserved' state for Booking {BookingId} when payment succeeded (Current Status: {Status}, Current BookingId: {CurrentBookingId}). Booking will be confirmed, but seat status remains unchanged.",
                        seatStatus.SeatId, booking.EventId, booking.Id, seatStatus.Status, seatStatus.CurrentBookingId);
                    // Business Decision: Confirm the booking anyway? Or fail it? Confirming seems reasonable but requires monitoring/reconciliation.
                }

                // --- Persist Changes ---
                _bookingRepository.Update(booking);
                if (seatSoldSuccessfully)
                {
                    _seatStatusRepository.Update(seatStatus);
                }
                // If !seatSoldSuccessfully, we don't call Update on seatStatus as it wasn't changed.

                await _dbContext.SaveChangesAsync(context.CancellationToken);
                await transaction.CommitAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Successfully processed PaymentSucceeded for Booking {BookingId}. Status Confirmed. Seat Sold: {SeatSold}",
                    message.BookingId, seatSoldSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing PaymentSucceeded for BookingId: {BookingId}. Rolling back transaction.",
                    message.BookingId);
                if (_dbContext.Database.CurrentTransaction != null)
                {
                    await transaction.RollbackAsync(context.CancellationToken);
                }

                throw; // Rethrow for MassTransit retry/error handling
            }
        }
    }
}