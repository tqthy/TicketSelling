using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Contracts.Infrastructure;

public class MockEventServiceApiClient : IEventServiceApiClient
{
    public Task<decimal> GetSeatPriceAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default)
    {
        // Return a mock price for the seat
        return Task.FromResult(75.00m); // Example: Fixed price
    }

    public Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        // Return mock event details
        var mockEvent = new EventDetailsDto
        {
            EventId = eventId,
            Name = "Mock Event",
            Date = DateTime.UtcNow.AddDays(7) // Example: Event scheduled for next week
        };
        return Task.FromResult<EventDetailsDto?>(mockEvent);
    }

    public Task<Dictionary<Guid, EventDetailsDto>> GetEventDetailsBatchAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        // Return mock details for a batch of events
        var mockEvents = new Dictionary<Guid, EventDetailsDto>();
        foreach (var eventId in eventIds)
        {
            mockEvents[eventId] = new EventDetailsDto
            {
                EventId = eventId,
                Name = $"Mock Event {eventId}",
                Date = DateTime.UtcNow.AddDays(7) // Example: All events scheduled for next week
            };
        }
        return Task.FromResult(mockEvents);
    }
}