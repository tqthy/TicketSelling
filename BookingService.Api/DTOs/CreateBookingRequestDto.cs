using System.ComponentModel.DataAnnotations;

namespace BookingService.Api.DTOs;

public class CreateBookingRequestDto
{
    [Required] public Guid EventId { get; set; }
    [Required] public required List<Guid> SeatIds { get; set; }
}