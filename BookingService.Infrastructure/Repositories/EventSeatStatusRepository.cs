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

        // SaveChanges would likely be handled by a UnitOfWork coordinating across repositories
    }
}