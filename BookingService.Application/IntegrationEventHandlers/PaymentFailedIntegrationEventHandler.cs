// BookingService.Application/IntegrationEventHandlers/PaymentFailedIntegrationEventHandler.cs

using MassTransit;
using Common.Messages; // Assuming PaymentFailed is here
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data; // For BookingDbContext
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For transaction
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Application.IntegrationEventHandlers
{
    public class PaymentFailedIntegrationEventHandler : IConsumer<PaymentFailed>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventSeatStatusRepository _seatStatusRepository;
        private readonly BookingDbContext _dbContext; // Inject DbContext for transaction and SaveChanges
        private readonly ILogger<PaymentFailedIntegrationEventHandler> _logger;

        public PaymentFailedIntegrationEventHandler(
            IBookingRepository bookingRepository,
            IEventSeatStatusRepository seatStatusRepository,
            BookingDbContext dbContext,
            ILogger<PaymentFailedIntegrationEventHandler> logger)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _seatStatusRepository = seatStatusRepository ?? throw new ArgumentNullException(nameof(seatStatusRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var message = context.Message;
            _logger.LogWarning("Received PaymentFailed event for BookingId: {BookingId}. Reason: {Reason}",
                message.BookingId, message.Reason);

            // Use a transaction for atomicity between Booking and SeatStatus updates
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

            try
            {
                // Fetch the booking, including seat info to find the EventSeatStatus
                var booking = await _bookingRepository.FindByIdAsync(message.BookingId, includeSeats: true);

                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found when processing PaymentFailed event. Ignoring message.", message.BookingId);
                    await transaction.RollbackAsync(context.CancellationToken); // Rollback is safe even if nothing changed
                    return;
                }

                // Idempotency check / State validation
                if (booking.Status != BookingStatus.PendingPayment)
                {
                    _logger.LogWarning("Received PaymentFailed for Booking {BookingId} which is not in PendingPayment status (Current: {Status}). Ignoring message.",
                                       message.BookingId, booking.Status);
                    await transaction.RollbackAsync(context.CancellationToken);
                    return;
                }

                // Get the specific seat involved (assuming one seat per booking for simplicity)
                // Adjust if a booking can have multiple seats that need releasing
                var bookedSeat = booking.BookedSeats.FirstOrDefault();
                if (bookedSeat == null)
                {
                     _logger.LogError("Inconsistent state: Booking {BookingId} has no BookedSeat associated during PaymentFailed processing. Rolling back.", message.BookingId);
                     await transaction.RollbackAsync(context.CancellationToken);
                     // Consider throwing an exception to move message to error queue for investigation
                     throw new InvalidOperationException($"Booking {message.BookingId} has no associated seat.");
                }

                // Fetch the corresponding seat status
                var seatStatus = await _seatStatusRepository.FindByEventAndSeatAsync(booking.EventId, bookedSeat.SeatId);

                if (seatStatus == null)
                {
                     _logger.LogError("Inconsistent state: EventSeatStatus not found for Event {EventId}, Seat {SeatId} associated with failed Booking {BookingId}. Rolling back.",
                                      booking.EventId, bookedSeat.SeatId, message.BookingId);
                     await transaction.RollbackAsync(context.CancellationToken);
                     throw new InvalidOperationException($"EventSeatStatus not found for booking {message.BookingId}.");
                }

                // --- Apply Domain Logic ---
                _logger.LogInformation("Updating Booking {BookingId} status to Failed.", message.BookingId);
                booking.FailPayment(); // Update booking aggregate state

                 try
                 {
                      // Attempt to make the seat available, but only if it's still reserved by *this* booking
                      _logger.LogInformation("Attempting to make Seat {SeatId} available due to failed payment for Booking {BookingId}.", seatStatus.SeatId, message.BookingId);
                      seatStatus.MakeAvailable(expectedBookingId: booking.Id); // Use domain method
                 }
                 catch (InvalidOperationException ex)
                 {
                      // Log if MakeAvailable throws (e.g., seat was already Sold) but don't fail the whole handler
                      _logger.LogWarning(ex, "Could not make Seat {SeatId} available for Booking {BookingId}. It might be already sold or in an unexpected state ({CurrentStatus}). Booking status will still be set to Failed.",
                                           seatStatus.SeatId, message.BookingId, seatStatus.Status);
                 }


                // --- Persist Changes ---
                _bookingRepository.Update(booking); // Mark booking as updated
                _seatStatusRepository.Update(seatStatus); // Mark seat status as updated (even if MakeAvailable did nothing, its state might need saving if tracked)

                await _dbContext.SaveChangesAsync(context.CancellationToken); // Save changes within transaction
                await transaction.CommitAsync(context.CancellationToken); // Commit transaction

                _logger.LogInformation("Successfully processed PaymentFailed for Booking {BookingId}. Status set to Failed.", message.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PaymentFailed for BookingId: {BookingId}. Reason: {Reason}. Rolling back transaction.",
                    message.BookingId, message.Reason);

                // Ensure rollback on any exception
                if (_dbContext.Database.CurrentTransaction != null) // Check if transaction is still active
                {
                    await transaction.RollbackAsync(context.CancellationToken);
                }

                // Rethrow the exception so MassTransit can handle retries/error queue
                throw;
            }
        }
    }
}