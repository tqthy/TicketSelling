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

    public static class BookingDbContextSeed
    {
        public static async Task SeedAsync(BookingDbContext context)
        {
            if (!context.Bookings.Any())
            {
                var booking1 = Booking.CreateForSeeding(
                    userId: Guid.NewGuid(),
                    eventId: Guid.NewGuid(),
                    totalPrice: 150.00m,
                    status: BookingStatus.PendingPayment,
                    expiresAt: DateTime.UtcNow.AddHours(1)
                );

                var booking2 = Booking.CreateForSeeding(
                    userId: Guid.NewGuid(),
                    eventId: Guid.NewGuid(),
                    totalPrice: 200.00m,
                    status: BookingStatus.Confirmed,
                    expiresAt: null
                );

                context.Bookings.AddRange(booking1, booking2);

                var bookedSeat1 = BookedSeat.CreateForSeeding(booking1.Id, Guid.NewGuid(), 75.00m);
                var bookedSeat2 = BookedSeat.CreateForSeeding(booking1.Id, Guid.NewGuid(), 75.00m);
                var bookedSeat3 = BookedSeat.CreateForSeeding(booking2.Id, Guid.NewGuid(), 200.00m);

                context.BookedSeats.AddRange(bookedSeat1, bookedSeat2, bookedSeat3);
            }

            if (!context.EventSeatStatuses.Any())
            {
                var eventSeatStatus1 = EventSeatStatus.CreateForSeeding(
                    eventId: Guid.NewGuid(),
                    seatId: Guid.NewGuid(),
                    status: "Reserved",
                    reservedUntil: DateTime.UtcNow.AddMinutes(30)
                );

                var eventSeatStatus2 = EventSeatStatus.CreateForSeeding(
                    eventId: Guid.NewGuid(),
                    seatId: Guid.NewGuid(),
                    status: "Available",
                    reservedUntil: null
                );

                context.EventSeatStatuses.AddRange(eventSeatStatus1, eventSeatStatus2);
            }

            await context.SaveChangesAsync();
        }
    }
}
