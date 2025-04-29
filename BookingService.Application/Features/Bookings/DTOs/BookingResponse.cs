namespace BookingService.Application.Features.Bookings.DTOs;

public class BookingResponse
{
    public Guid BookingId { get; set; }
    public string Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingInitiatedAt { get; set; }
    public DateTime PaymentDeadline { get; set; }
}