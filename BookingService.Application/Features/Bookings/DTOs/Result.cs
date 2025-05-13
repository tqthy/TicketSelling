namespace BookingService.Application.Features.Bookings.DTOs;

public class Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
}