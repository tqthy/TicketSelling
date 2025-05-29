using BookingService.Domain.AggregatesModel.BookingAggregate;

namespace BookingService.Api.DTOs;

public class UpdateBookingStatusRequestDto
{
    public string Status { get; set; }
}