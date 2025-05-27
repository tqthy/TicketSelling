namespace BookingService.Application.Features.Bookings.DTOs;

public class BookedSeatDetailDto
{
    public string SeatId { get; set; } = string.Empty;
    public decimal PriceAtBooking { get; set; }
}