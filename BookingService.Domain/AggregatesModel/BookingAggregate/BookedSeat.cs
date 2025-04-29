using Common.SeedWork;

namespace BookingService.Domain.AggregatesModel.BookingAggregate;

public class BookedSeat : BaseEntity<Guid>
{
     public Guid BookingId { get; set; } // FK to Booking
     public virtual Booking Booking { get; set; }
     public Guid SeatId { get; set; } // Link to the actual seat (potentially in VenueService)
     public decimal PriceAtBooking { get; set; }
     
     private BookedSeat(){}
     public BookedSeat(Guid bookingId, Guid seatId, decimal priceAtBooking)
     {
         BookingId = bookingId;
         SeatId = seatId;
         PriceAtBooking = priceAtBooking;
     }
}