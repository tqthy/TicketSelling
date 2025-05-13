namespace BookingService.Application.Features.Events.DTOs;

public class EventSeatDetailDto
{
    public Guid SeatId { get; set; }
    public decimal Price { get; set; }
    public string? SeatNumber { get; set; }
    public string? Row { get; set; }
    public string? SectionName { get; set; }
    public string Status { get; set; }
    public string SectionId { get; set; } 
}