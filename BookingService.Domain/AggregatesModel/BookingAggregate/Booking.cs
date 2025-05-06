
using System;
using System.Collections.Generic;
using System.Linq;
using Common.SeedWork;

// Assuming BaseEntity exists for common properties like Id/DomainEvents
// using Common.SeedWork; // Or define base types here

namespace BookingService.Domain.AggregatesModel.BookingAggregate
{
    // Using BookingId as the primary identifier for the Aggregate Root
    public class Booking : BaseEntity<Guid>, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid EventId { get; private set; }
        public string Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public DateTime? ExpiresAt { get; private set; } // Reservation expiry

        // Navigation property for owned entities
        private readonly List<BookedSeat> _bookedSeats = new List<BookedSeat>();
        public IReadOnlyCollection<BookedSeat> BookedSeats => _bookedSeats.AsReadOnly();

        // Private constructor for EF Core/persistence
        private Booking() : base() { }

        // Factory method or public constructor for creation
        public Booking(Guid userId, Guid eventId, decimal totalPrice, TimeSpan reservationDuration)
            : base(Guid.NewGuid()) // Pass the generated Id to the BaseEntity constructor
        {
            UserId = userId;
            EventId = eventId;
            // Add domain event using the method from BaseEntity
            // AddDomainEvent(new BookingInitiatedDomainEvent( /* event details */ ));
        }


        // Methods to manipulate aggregate state (Domain Logic)
        public void AddSeat(Guid seatId, decimal priceAtBooking)
        {
            if (Status != BookingStatus.PendingPayment)
            {
                throw new InvalidOperationException("Cannot add seats to a booking not in PendingPayment status.");
            }
            // Prevent adding duplicates 
            if (!_bookedSeats.Any(s => s.SeatId == seatId))
            {
                var bookedSeat = new BookedSeat(this.Id, seatId, priceAtBooking);
                _bookedSeats.Add(bookedSeat);
            }
        }

        public void ConfirmPayment()
        {
            if (Status == BookingStatus.PendingPayment)
            {
                Status = BookingStatus.Confirmed;
                ExpiresAt = null; // No longer expires
                // Optional: Add Domain Event
                // AddDomainEvent(new BookingConfirmedDomainEvent(this.Id));
            }
            else
            {
                 // Log or handle idempotency - already confirmed? Or in invalid state?
                 Console.WriteLine($"Warning: Attempted to confirm booking {Id} which has status {Status}.");
            }
        }

        public void FailPayment()
        {
            if (Status == BookingStatus.PendingPayment)
            {
                Status = BookingStatus.Failed;
                ExpiresAt = null;
                 // Optional: Add Domain Event
                // AddDomainEvent(new BookingFailedDomainEvent(this.BookingId));
            }
            else
            {
                 Console.WriteLine($"Warning: Attempted to fail booking {Id} which has status {Status}.");
            }
        }

        // Add other domain methods as needed...
    }
}