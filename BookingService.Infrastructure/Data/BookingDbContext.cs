// Likely located in BookingService.Infrastructure/Data/BookingDbContext.cs

using BookingService.Domain.AggregatesModel.BookingAggregate; // Namespace for EventSeatStatus
using Microsoft.EntityFrameworkCore;
// Potentially other using statements for BaseEntity, IUnitOfWork etc.

namespace BookingService.Infrastructure.Data
{
    public class BookingDbContext : DbContext // Could also implement IUnitOfWork
    {
        // DbSet for the Booking Aggregate Root
        public DbSet<Booking> Bookings { get; set; }

        // DbSet for the BookedSeat Entity (part of the Booking Aggregate)
        public DbSet<BookedSeat> BookedSeats { get; set; }
        
        public DbSet<EventSeatStatus> EventSeatStatuses { get; set; }

        // Constructor used for Dependency Injection
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);

            // Or configure entities directly here (Example for EventSeatStatus composite key):
            modelBuilder.Entity<EventSeatStatus>(entity =>
            {
                // Define composite primary key
                entity.HasKey(e => new { e.EventId, e.SeatId });

                // Configure properties (like string length, required, etc.)
                entity.Property(e => e.Status)
                      .HasMaxLength(50) // Example length
                      .IsRequired();

                // Configure indexes (IMPORTANT for performance of lookups)
                entity.HasIndex(e => new { e.EventId, e.SeatId }).IsUnique(); // Index on PK
                entity.HasIndex(e => e.Status); // Index status if frequently queried
                entity.HasIndex(e => e.ReservedUntil); // Index if queried for expiration checks
            });
        }

        // Optional: Override SaveChangesAsync if implementing UnitOfWork or dispatching Domain Events here
        // public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { ... }
    }
}