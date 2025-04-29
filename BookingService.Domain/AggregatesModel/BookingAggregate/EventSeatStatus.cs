using System;
using Common.SeedWork; // Optional: If inheriting from a base or using common types

namespace BookingService.Domain.AggregatesModel.BookingAggregate
{
    /// <summary>
    /// Represents the availability status of a specific Seat for a specific Event.
    /// This entity's identity is often defined by the composite of EventId and SeatId.
    /// </summary>
    // Note: Doesn't inherit BaseEntity<TId> as its primary key in the DB is likely composite (EventId, SeatId).
    // EF Core can handle composite keys without a dedicated Id property here.
    // It's also not an Aggregate Root itself.
    public class EventSeatStatus
    {
        // Properties forming the composite key (configure in Infrastructure DbContext)
        public Guid EventId { get; private set; }
        public Guid SeatId { get; private set; }

        /// <summary>
        /// Current status of the seat (e.g., Available, Reserved, Sold).
        /// Use the SeatAvailabilityStatus constants/enum.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// If reserved, the time until which the reservation is held. Null otherwise.
        /// </summary>
        public DateTime? ReservedUntil { get; private set; }

        /// <summary>
        /// If reserved or sold, the ID of the booking associated with the current reservation or sale.
        /// </summary>
        public Guid? CurrentBookingId { get; private set; }

        // Constructor needed for EF Core
        private EventSeatStatus() { }

        // Optional: Public constructor if needed, ensure valid initial state
        public EventSeatStatus(Guid eventId, Guid seatId, string initialStatus = SeatAvailabilityStatus.Available)
        {
            EventId = eventId;
            SeatId = seatId;
            Status = initialStatus; // Default to Available
            ReservedUntil = null;
            CurrentBookingId = null;
        }

        // --- Domain Logic Methods ---

        /// <summary>
        /// Reserves the seat for a specific booking until a given time.
        /// Throws InvalidOperationException if the seat is not currently available or its reservation expired.
        /// </summary>
        /// <param name="bookingId">The ID of the booking reserving the seat.</param>
        /// <param name="reservationEndTime">The UTC time when the reservation expires.</param>
        public void Reserve(Guid bookingId, DateTime reservationEndTime)
        {
            // Check if it's available via the lazy check logic
            if (!IsAvailableOrExpiredReservation(DateTime.UtcNow))
            {
                throw new InvalidOperationException($"Seat {SeatId} for event {EventId} cannot be reserved because its current status is '{Status}'.");
            }

            Status = SeatAvailabilityStatus.Reserved;
            CurrentBookingId = bookingId;
            ReservedUntil = reservationEndTime;
        }

        /// <summary>
        /// Marks the seat as sold to the specified booking.
        /// Typically called when payment is confirmed.
        /// Throws InvalidOperationException if the seat is not currently reserved by the given booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking purchasing the seat.</param>
        public void Sell(Guid bookingId)
        {
            // Usually, a seat should be Reserved by this booking before it can be Sold
            if (Status != SeatAvailabilityStatus.Reserved || CurrentBookingId != bookingId)
            {
                 throw new InvalidOperationException($"Seat {SeatId} for event {EventId} cannot be marked as sold for booking {bookingId} because its current status is '{Status}' or it's reserved for booking '{CurrentBookingId}'.");
            }

            Status = SeatAvailabilityStatus.Sold;
            ReservedUntil = null; // Clear reservation expiry
            // CurrentBookingId remains set to the booking that purchased it.
        }

        /// <summary>
        /// Makes the seat available, clearing any reservation details.
        /// Typically called when a payment fails or a reservation expires and needs explicit cleanup.
        /// Throws InvalidOperationException if the seat is already sold.
        /// </summary>
         public void MakeAvailable(Guid? expectedBookingId = null)
         {
             if (Status == SeatAvailabilityStatus.Sold)
             {
                  throw new InvalidOperationException($"Cannot make seat {SeatId} for event {EventId} available as it is already sold (Booking: {CurrentBookingId}).");
             }

             // Optional: Only make available if it's reserved by the expected booking
             if (expectedBookingId.HasValue && CurrentBookingId != expectedBookingId.Value)
             {
                 // Don't change status if it's reserved by someone else already
                 Console.WriteLine($"Warning: Attempted to make seat {SeatId} available, but it was expected to be reserved by {expectedBookingId} and is actually reserved by {CurrentBookingId}. Status not changed.");
                 return;
             }

             Status = SeatAvailabilityStatus.Available;
             ReservedUntil = null;
             CurrentBookingId = null;
         }


        /// <summary>
        /// Checks if the seat is currently available for booking,
        /// considering both 'Available' status and expired reservations ('lazy check').
        /// </summary>
        /// <param name="currentTimeUtc">The current UTC time to check against expiry.</param>
        /// <returns>True if the seat can be considered available for a new reservation.</returns>
        public bool IsAvailableOrExpiredReservation(DateTime currentTimeUtc)
        {
            return Status == SeatAvailabilityStatus.Available ||
                   (Status == SeatAvailabilityStatus.Reserved && ReservedUntil.HasValue && ReservedUntil.Value < currentTimeUtc);
        }
    }

    /// <summary>
    /// Defines standard status values for seat availability.
    /// Using a static class for constants is simple; an Enumeration class (from SeedWork) offers more features.
    /// </summary>
    public static class SeatAvailabilityStatus
    {
        public const string Available = "Available";
        public const string Reserved = "Reserved";
        public const string Sold = "Sold";
        // Add other statuses like "Locked", "Unavailable" if needed
    }
}