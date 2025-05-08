using BookingService.Application.Features.Events.DTOs;
using MediatR;

namespace BookingService.Application.Features.Events.Queries;

public class GetEventSeatsQuery : IRequest<List<EventSeatDetailDto>>
{
    public Guid EventId { get; set; }
}