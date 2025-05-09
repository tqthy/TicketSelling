using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        private readonly ILogger<BookingRepository> _logger;

        public BookingRepository(BookingDbContext context, ILogger<BookingRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Finds a Booking by its ID, optionally including its associated BookedSeats.
        /// </summary>
        /// <param name="bookingId">The Guid ID of the booking.</param>
        /// <param name="includeSeats">Whether to include the BookedSeats collection.</param>
        /// <returns>The found Booking or null.</returns>
        public async Task<Booking?> FindByIdAsync(Guid bookingId, bool includeSeats = false)
        {
            _logger.LogDebug("Finding Booking by Id {BookingId}. IncludeSeats: {IncludeSeats}", bookingId, includeSeats);

            IQueryable<Booking> query = _context.Bookings;

            if (includeSeats)
            {
                query = query.Include(b => b.BookedSeats); // Eager load seats if requested
            }

            // Assuming Booking inherits from BaseEntity<Guid>, the primary key is 'Id'
            return await query.FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        /// <summary>
        /// Finds all Bookings for a specific user, including their BookedSeats.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of the user's bookings.</returns>
        public async Task<List<Booking>> FindByUserIdAsync(Guid userId)
        {
            _logger.LogDebug("Finding Bookings for User {UserId}", userId);

            // Usually when fetching bookings for a list, seats are needed.
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.BookedSeats) // Eager load seats
                .OrderByDescending(b => b.CreatedAt) // Order by creation date, newest first
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new Booking entity to the DbContext.
        /// Changes are not saved until SaveChangesAsync is called (likely via Unit of Work).
        /// </summary>
        /// <param name="booking">The booking entity to add.</param>
        public void Add(Booking booking)
        {
            _logger.LogDebug("Adding new Booking {BookingId} to context.", booking.Id);
            // EF Core tracks the entity and marks its state as 'Added'
            _context.Bookings.Add(booking);
        }

        /// <summary>
        /// Marks an existing Booking entity as modified in the DbContext.
        /// Changes are not saved until SaveChangesAsync is called (likely via Unit of Work).
        /// </summary>
        /// <param name="booking">The booking entity to update.</param>
        public void Update(Booking booking)
        {
             _logger.LogDebug("Marking Booking {BookingId} as updated in context.", booking.Id);
            // Ensures EF Core knows the entity might have changes.
            // If the entity was fetched and modified within the same context scope,
            // EF Core's change tracking might already know it's modified, but
            // explicitly calling Update or SetModified is safer.
            _context.Entry(booking).State = EntityState.Modified;
            // Alternatively, _context.Bookings.Update(booking); also works but can sometimes
            // mark all properties as modified, potentially causing less efficient SQL updates.
            // Setting the state is often preferred if you know the entity is already tracked.
        }

        public Task<IEnumerable<object>> GetBookingsByUserIdAsync(Guid requestUserId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}