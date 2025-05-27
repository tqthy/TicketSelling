using System;
using System.Collections.Generic;
using System.Linq;
using Common.SeedWork;

namespace BookingService.Domain.AggregatesModel.BookingAggregate
{
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
        public Booking() : base() { }

        // Factory method or public constructor for creation
        public Booking(Guid userId, Guid eventId, decimal totalPrice, TimeSpan reservationDuration)
            : base(Guid.NewGuid()) // Pass the generated Id to the BaseEntity constructor
        {
            UserId = userId;
            EventId = eventId;
            Status = BookingStatus.PendingPayment;
            TotalPrice = totalPrice;
            ExpiresAt = DateTime.UtcNow.Add(reservationDuration); 
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

        // Internal method for seeding or controlled updates
        internal static Booking CreateForSeeding(Guid userId, Guid eventId, decimal totalPrice, string status, DateTime? expiresAt)
        {
            return new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = eventId,
                TotalPrice = totalPrice,
                Status = status,
                ExpiresAt = expiresAt
            };
        }
    }
}
