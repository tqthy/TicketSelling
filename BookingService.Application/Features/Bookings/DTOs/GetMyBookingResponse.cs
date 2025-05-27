namespace BookingService.Application.Features.Bookings.DTOs;

public class GetMyBookingResponse
{
    public string Id { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<BookedSeatDetailDto> BookedSeats { get; set; } = new List<BookedSeatDetailDto>();
}