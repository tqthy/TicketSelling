using System;
using System.Threading.Tasks;

namespace BookingService.Domain.AggregatesModel.BookingAggregate
{
    /// <summary>
    /// Interface defining repository operations for EventSeatStatus entities.
    /// </summary>
    public interface IEventSeatStatusRepository
    {
        /// <summary>
        /// Finds an EventSeatStatus by its composite key (EventId, SeatId).
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="seatId">The ID of the seat.</param>
        /// <returns>The found EventSeatStatus or null if not found.</returns>
        Task<EventSeatStatus?> FindByEventAndSeatAsync(Guid eventId, Guid seatId);

        /// <summary>
        /// Finds an EventSeatStatus by its composite key with the intent to update,
        /// potentially acquiring a database lock (depending on implementation).
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="seatId">The ID of the seat.</param>
        /// <returns>The found EventSeatStatus ready for update, or null if not found.</returns>
        Task<EventSeatStatus?> FindByEventAndSeatForUpdateAsync(Guid eventId, Guid seatId);

        /// <summary>
        /// Finds an EventSeatStatus by its composite key with the intent to update,
        /// potentially acquiring a database lock (depending on implementation).
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="seatIds">The IDs of the seat.</param>
        /// <returns>The found EventSeatStatus ready for update, or null if not found.</returns>
        Task<List<EventSeatStatus>> FindByEventAndSeatsForUpdateAsync(Guid eventId, List<Guid> seatIds);

        /// <summary>
        /// Marks an EventSeatStatus entity as modified in the Unit of Work.
        /// </summary>
        /// <param name="seatStatus">The entity to mark for update.</param>
        void Update(EventSeatStatus seatStatus);

        // Note: SaveChangesAsync is typically handled by a Unit of Work pattern
        // coordinating changes across multiple repositories, so it's often not included here.
        Task UpdateSeatStatuses(Guid bookingId, List<Guid> seatIds, Guid eventId ,string requestStatus);
    }
}