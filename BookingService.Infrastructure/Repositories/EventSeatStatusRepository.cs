using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Repositories
{
    public class EventSeatStatusRepository : IEventSeatStatusRepository
    {
        private readonly BookingDbContext _context;
        private readonly ILogger<EventSeatStatusRepository> _logger; // Add logging

        public EventSeatStatusRepository(BookingDbContext context, ILogger<EventSeatStatusRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EventSeatStatus?> FindByEventAndSeatAsync(Guid eventId, Guid seatId)
        {
            return await _context.EventSeatStatuses
                .FirstOrDefaultAsync(s => s.EventId == eventId && s.SeatId == seatId);
        }

        public async Task<EventSeatStatus?> FindByEventAndSeatForUpdateAsync(Guid eventId, Guid seatId)
        {
            // --- Implementation of Pessimistic Locking ---
            try
            {
                // Ensure EventId and SeatId are indexed together for performance
                return await _context.EventSeatStatuses
                    .FromSqlRaw("SELECT * FROM \"EventSeatStatuses\" WHERE \"EventId\" = {0} AND \"SeatId\" = {1} FOR UPDATE", eventId, seatId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing FOR UPDATE query for Event {EventId}, Seat {SeatId}", eventId, seatId);
                // Depending on DB/provider, locking errors might throw specific exceptions
                throw; // Re-throw to allow transaction rollback
            }

            // Alternative: Using EF Core's UsePessimisticLocking (if available for your provider)
            // return await _context.EventSeatStatuses
            //     .Where(s => s.EventId == eventId && s.SeatId == seatId)
            //     .UsePessimisticLocking(PessimisticLock.Update) // Example syntax
            //     .FirstOrDefaultAsync();

             // Alternative: Optimistic Concurrency requires RowVersion checks during SaveChangesAsync
        }
        
        public async Task<List<EventSeatStatus>> FindByEventAndSeatsForUpdateAsync(Guid eventId, List<Guid> seatIds)
        {
            try
            {
                // Use LINQ with EF Core for better readability and maintainability
                return await _context.EventSeatStatuses
                    .Where(ess => ess.EventId == eventId && seatIds.Contains(ess.SeatId))
                    .OrderBy(ess => ess.SeatId)
                    .TagWith("FOR UPDATE")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching EventSeatStatuses for Event {EventId} and Seats {SeatIds}", eventId, string.Join(", ", seatIds));
                throw; // Re-throw to allow transaction rollback
            }
        }

        public void Update(EventSeatStatus seatStatus)
        {
            _context.EventSeatStatuses.Update(seatStatus);
        }

        public async Task UpdateSeatStatuses(Guid bookingId, List<Guid> seatIds, Guid eventId, string requestStatus)
        {
            try
            {
                _logger.LogInformation("Updating seat statuses for Booking {BookingId}, Event {EventId}, Seats {SeatIds}",
                    bookingId, eventId, string.Join(", ", seatIds));
                foreach (var seatId in seatIds)
                {
                    // Find the seat status for the booked seat
                    var seatStatus = _context.EventSeatStatuses
                        .FirstOrDefault(s => s.EventId == eventId && s.SeatId == seatId);

                    if (seatStatus != null)
                    {
                        // Update the status based on the booking request status
                        if (requestStatus == SeatAvailabilityStatus.Reserved)
                        {
                            seatStatus.Reserve(bookingId, DateTime.UtcNow.AddMinutes(15)); // Example reservation time
                        }
                        else if (requestStatus == SeatAvailabilityStatus.Sold)
                        {
                            seatStatus.Sell(bookingId);
                        }
                        else if (requestStatus == SeatAvailabilityStatus.Available)
                        {
                            // If the status is set to Available, we might want to clear the booking ID and reserved time
                            seatStatus.MakeAvailable(bookingId);
                        }
                        
                        _context.EventSeatStatuses.Update(seatStatus);
                        await _context.SaveChangesAsync(); 
                    }
                    else
                    {
                        _logger.LogWarning("EventSeatStatus not found for Event {EventId}, Seat {SeatId}", eventId,
                            seatId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seat statuses for Booking {BookingId}, Event {EventId}, Seats {SeatIds}",
                    bookingId, eventId, string.Join(", ", seatIds));
                throw; // Re-throw to allow transaction rollback
            }
        }


        // SaveChanges would likely be handled by a UnitOfWork coordinating across repositories
    }
}