using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// using Common.SeedWork; 

namespace BookingService.Domain.AggregatesModel.BookingAggregate
{
    // Inherit IUnitOfWork or similar if bundle SaveChanges here
    public interface IBookingRepository // : IRepository<Booking>
    {
        Task<Booking?> FindByIdAsync(Guid bookingId, bool includeSeats = false); // Allow including related data
        Task<List<Booking>> FindByUserIdAsync(Guid userId);
        void Add(Booking booking);
        void Update(Booking booking);
        // SaveChanges might be part of a UnitOfWork pattern instead of each repository
        // Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<object>> GetBookingsByUserIdAsync(Guid requestUserId, CancellationToken cancellationToken);
    }
}