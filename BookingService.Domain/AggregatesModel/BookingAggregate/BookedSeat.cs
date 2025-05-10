using Common.SeedWork;

namespace BookingService.Domain.AggregatesModel.BookingAggregate;

public class BookedSeat : BaseEntity<Guid>
{
    public Guid BookingId { get; private set; } // Changed to private setter
    public virtual Booking Booking { get; private set; } // Changed to private setter
    public Guid SeatId { get; private set; } // Changed to private setter
    public decimal PriceAtBooking { get; private set; } // Changed to private setter

    private BookedSeat() { }

    public BookedSeat(Guid bookingId, Guid seatId, decimal priceAtBooking)
    {
        BookingId = bookingId;
        SeatId = seatId;
        PriceAtBooking = priceAtBooking;
    }

    // Internal method for seeding or controlled updates
    internal static BookedSeat CreateForSeeding(Guid bookingId, Guid seatId, decimal priceAtBooking)
    {
        return new BookedSeat
        {
            BookingId = bookingId,
            SeatId = seatId,
            PriceAtBooking = priceAtBooking
        };
    }
}
