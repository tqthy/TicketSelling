using MediatR;
using MassTransit; // For IPublishEndpoint
using Common.Messages; // For BookingInitiated integration event
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Application.Contracts.Infrastructure; // For API Clients
using BookingService.Application.Features.Bookings.DTOs; // For response DTO
using System;
using System.Threading;
using System.Threading.Tasks;
using BookingService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException, BeginTransactionAsync

// Assuming IUnitOfWork for transaction management
// using BookingService.Application.Contracts.Persistence;

namespace BookingService.Application.Features.Bookings.Commands
{

    // Define a result object for better handling in API layer
    public class BookingResult : Result
    {
        public BookingResponse? BookingDetails { get; set; }
        public bool IsConflict { get; set; } // Flag for 409 Conflict
    }

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventSeatStatusRepository _seatStatusRepository;
        private readonly IEventServiceApiClient _eventServiceApiClient; // Use interface
        private readonly IPublishEndpoint _publishEndpoint; // For Integration Events
        private readonly ILogger<CreateBookingCommandHandler> _logger;
        private readonly BookingDbContext _dbContext; // Inject DbContext for Transaction Control (could use IUnitOfWork abstraction)

        private static readonly TimeSpan ReservationTimeout = TimeSpan.FromMinutes(10);

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IEventSeatStatusRepository seatStatusRepository,
            IEventServiceApiClient eventServiceApiClient,
            IPublishEndpoint publishEndpoint,
            BookingDbContext dbContext, // Inject DbContext directly for transaction control
            ILogger<CreateBookingCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _seatStatusRepository = seatStatusRepository;
            _eventServiceApiClient = eventServiceApiClient;
            _publishEndpoint = publishEndpoint;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<BookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        try
        {
            if (request.SeatIds.Count == 0)
            {
                return new BookingResult { Success = false, ErrorMessage = "No seat IDs provided for booking." };
            }
            var distinctSeatIds = request.SeatIds.Distinct().ToList(); // Process unique seats

            // 1. Fetch SeatStatuses with lock for all requested seats
            var seatStatuses = await _seatStatusRepository.FindByEventAndSeatsForUpdateAsync(request.EventId, distinctSeatIds);

            if (seatStatuses.Count != distinctSeatIds.Count)
            {
                var foundSeatIds = seatStatuses.Select(s => s.SeatId).ToList();
                var missingSeatIds = distinctSeatIds.Except(foundSeatIds).ToList();
                _logger.LogWarning("Not all seat statuses found for Event {EventId}. Requested: {RequestedSeatIds}, Missing: {MissingSeatIds}",
                    request.EventId, distinctSeatIds, missingSeatIds);
                await transaction.RollbackAsync(cancellationToken);
                return new BookingResult { Success = false, ErrorMessage = $"Seat status information not found for seats: {string.Join(", ", missingSeatIds)}." };
            }

            // 2. Perform lazy check for all seats
            var unavailableSeats = new List<Guid>();
            foreach (var seatStatus in seatStatuses)
            {
                if (!seatStatus.IsAvailableOrExpiredReservation(DateTime.UtcNow))
                {
                    unavailableSeats.Add(seatStatus.SeatId);
                }
            }

            if (unavailableSeats.Any())
            {
                _logger.LogWarning("Seats {UnavailableSeatIds} for Event {EventId} are not available. Rolling back.", unavailableSeats, request.EventId);
                await transaction.RollbackAsync(cancellationToken);
                return new BookingResult { Success = false, IsConflict = true, ErrorMessage = $"Seats not available or already reserved: {string.Join(", ", unavailableSeats)}." };
            }

            // 3. Get Price for each seat and calculate total price
            decimal totalPrice = seatStatuses.Sum(s => s.SeatPrice);
            

            // 4. Create Booking Aggregate
            var booking = new Booking(request.UserId, request.EventId, totalPrice, ReservationTimeout);

            foreach (var seat in seatStatuses)
            {
                booking.AddSeat(seat.SeatId, seat.SeatPrice);
            }
            
            if (!booking.ExpiresAt.HasValue) // Should be set by Booking constructor
            {
                 _logger.LogError("Booking Expiration was not set for Booking {BookingId}. Critical error.", booking.Id);
                 await transaction.RollbackAsync(cancellationToken);
                 return new BookingResult { Success = false, ErrorMessage = "Internal error setting booking expiration." };
            }
            DateTime reservationExpiry = booking.ExpiresAt.Value;

            // 5. Update Seat Statuses
            foreach (var seatStatus in seatStatuses)
            {
                seatStatus.Reserve(booking.Id, reservationExpiry);
            }

            // 6. Persist changes
            _bookingRepository.Add(booking);
            foreach (var seatStatus in seatStatuses) // Explicitly mark for update if repository pattern requires it
            {
                _seatStatusRepository.Update(seatStatus);
            }
            // If using EF Core and entities are tracked, the above _seatStatusRepository.Update loop
            // might be redundant if Reserve() modifies properties EF Core tracks.
            // However, it's safer to be explicit if the original pattern indicated it.

            // 7. Save changes within the transaction
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 8. Commit Transaction
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Booking {BookingId} for seats {SeatIds} initiated successfully for User {UserId}.", booking.Id, distinctSeatIds, request.UserId);

            // 9. Publish Integration Event (AFTER successful commit)
            var paymentMessage = new InitiatePaymentRequested(
                BookingId: booking.Id,
                UserId: booking.UserId,
                OrderType: "TicketBooking",
                Amount: booking.TotalPrice,
                Currency: "VND", // Example: Get this from event/system configuration
                OrderInfo: $"Payment for booking of seats {string.Join(", ", distinctSeatIds)} for event {request.EventId}",
                PreferredGateway: PaymentGatewaySelection.VnPay, // Or determine based on user/event preference
                IpAddress: request.UserIpAddress,
                Timestamp: DateTime.UtcNow
            );

            await _publishEndpoint.Publish(paymentMessage, cancellationToken);
            _logger.LogInformation("Published InitiatePaymentRequested event for BookingId: {BookingId}", booking.Id);

            // 10. Map result to Response DTO
            var responseDto = new BookingResponse
            {
                BookingId = booking.Id,
                Status = booking.Status,
                TotalPrice = booking.TotalPrice,
                BookingInitiatedAt = booking.CreatedAt,
                PaymentDeadline = reservationExpiry,
                // SeatIds = distinctSeatIds // Optionally include this
            };
            return new BookingResult { Success = true, BookingDetails = responseDto };
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex) // Be specific if using EF Core
        {
            var seatIdsString = request.SeatIds != null ? string.Join(", ", request.SeatIds) : "N/A";
            _logger.LogWarning(ex, "Concurrency conflict during booking for Event {EventId}, Seats [{SeatIds}]. Rolling back.", request.EventId, seatIdsString);
            await transaction.RollbackAsync(cancellationToken);
            return new BookingResult { Success = false, IsConflict = true, ErrorMessage = "The status of one or more seats changed. Please try again." };
        }
        catch (Exception ex)
        {
            var seatIdsString = request.SeatIds != null ? string.Join(", ", request.SeatIds) : "N/A";
            _logger.LogError(ex, "Unhandled exception during booking for Event {EventId}, Seats [{SeatIds}]. Rolling back.", request.EventId, seatIdsString);
            await transaction.RollbackAsync(cancellationToken);
            return new BookingResult { Success = false, ErrorMessage = "An unexpected error occurred while processing your booking." };
        }
    }
        // public async Task<BookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken = default)
        // {
        //     // --- Use DbContext transaction for atomicity ---
        //     await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        //
        //     try
        //     {
        //         // 1. Fetch SeatStatus with lock
        //         var seatStatus = await _seatStatusRepository.FindByEventAndSeatForUpdateAsync(request.EventId, request.SeatId);
        //
        //         if (seatStatus == null)
        //         {
        //             _logger.LogWarning("Seat status not found for Event {EventId}, Seat {SeatId}", request.EventId, request.SeatId);
        //             await transaction.RollbackAsync(cancellationToken);
        //             return new BookingResult { Success = false, ErrorMessage = "Seat status information not found." }; // Or NotFound status code hint
        //         }
        //
        //         // 2. Perform lazy check (Domain logic - could be in a Domain Service if more complex)
        //         bool isAvailable = seatStatus.IsAvailableOrExpiredReservation(DateTime.UtcNow); // Move check logic to EventSeatStatus entity
        //
        //         if (isAvailable)
        //         {
        //             // 3. Get Price
        //             decimal price;
        //             try
        //             {
        //                 // TODO: Call external service to get price
        //                 // price = await _eventServiceApiClient.GetSeatPriceAsync(request.EventId, request.SeatId); // Call external service
        //                  price = 50.00m; // Placeholder
        //                  if (price <= 0) throw new InvalidOperationException("Invalid price.");
        //             }
        //             catch (Exception ex)
        //             {
        //                 _logger.LogError(ex, "Failed to get price for Event {EventId}, Seat {SeatId}. Rolling back.", request.EventId, request.SeatId);
        //                 await transaction.RollbackAsync(cancellationToken);
        //                  return new BookingResult { Success = false, ErrorMessage = "Failed to retrieve pricing information." }; // Or ServiceUnavailable hint
        //             }
        //
        //             // 4. Create Booking Aggregate
        //             var booking = new Booking(request.UserId, request.EventId, price, ReservationTimeout);
        //             booking.AddSeat(request.SeatId, price); // Add seat via aggregate method
        //
        //             // 5. Update Seat Status (Domain logic delegated to entity method)
        //             seatStatus.Reserve(booking.Id, booking.ExpiresAt ?? DateTime.UtcNow.Add(ReservationTimeout)); // Update via entity method
        //
        //             // 6. Persist changes via Repositories
        //             _bookingRepository.Add(booking);
        //             _seatStatusRepository.Update(seatStatus); // Mark for update
        //
        //             // 7. Save changes within the transaction
        //             await _dbContext.SaveChangesAsync(cancellationToken); // Save changes coordinated by DbContext/UnitOfWork
        //
        //             // 8. Commit Transaction
        //             await transaction.CommitAsync(cancellationToken);
        //             _logger.LogInformation("Booking {BookingId} initiated successfully.", booking.Id);
        //
        //             // 9. Publish Integration Event (AFTER successful commit)
        //             var paymentMessage = new InitiatePaymentRequested(
        //                 BookingId: booking.Id,
        //                 UserId: booking.UserId,
        //                 OrderType: "TicketBooking", // Or more specific
        //                 Amount: booking.TotalPrice,
        //                 Currency: "VND", // Example: Get this from event/system configuration
        //                 OrderInfo: $"Payment for booking of seat {request.SeatId} for event {request.EventId}",
        //                 PreferredGateway: PaymentGatewaySelection.VnPay, // Or determine based on user/event preference
        //                 IpAddress: request.UserIpAddress, // Assuming this is passed in CreateBookingCommand
        //                 Timestamp: DateTime.UtcNow
        //             );
        //
        //             await _publishEndpoint.Publish(paymentMessage, cancellationToken);
        //             _logger.LogInformation("Published InitiatePaymentRequested event for BookingId: {BookingId}", booking.Id);
        //
        //
        //             // 10. Map result to Response DTO
        //             var responseDto = new BookingResponse
        //             {
        //                 BookingId = booking.Id,
        //                 Status = booking.Status,
        //                 TotalPrice = booking.TotalPrice,
        //                 BookingInitiatedAt = booking.CreatedAt,
        //                 PaymentDeadline = booking.ExpiresAt.Value
        //             };
        //             return new BookingResult { Success = true, BookingDetails = responseDto };
        //         }
        //         else // Seat not available
        //         {
        //             _logger.LogWarning("Seat {SeatId} for Event {EventId} is not available. Rolling back.", request.SeatId, request.EventId);
        //             await transaction.RollbackAsync(cancellationToken);
        //             return new BookingResult { Success = false, IsConflict = true, ErrorMessage = "Seat not available or already reserved." };
        //         }
        //     }
        //     catch (DbUpdateConcurrencyException ex) // Handle concurrency conflicts if using optimistic locking
        //     {
        //          _logger.LogWarning(ex, "Concurrency conflict during booking for Event {EventId}, Seat {SeatId}. Rolling back.", request.EventId, request.SeatId);
        //          await transaction.RollbackAsync(cancellationToken); // Ensure rollback
        //          return new BookingResult { Success = false, IsConflict = true, ErrorMessage = "The seat status changed. Please try again." };
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Unhandled exception during booking for Event {EventId}, Seat {SeatId}. Rolling back.", request.EventId, request.SeatId);
        //          await transaction.RollbackAsync(cancellationToken); // Ensure rollback
        //         return new BookingResult { Success = false, ErrorMessage = "An unexpected error occurred." }; // Generic error for API
        //     }
        // }
    }
}