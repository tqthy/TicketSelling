using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingService.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Interface abstracting communication with the external Event Service API.
    /// </summary>
    public interface IEventServiceApiClient
    {
        /// <summary>
        /// Gets the price for a specific seat or section within an event.
        /// </summary>
        /// <remarks>
        /// The exact parameters might change depending on how pricing is determined
        /// in the Event service (e.g., might need SectionId instead of/in addition to SeatId).
        /// </remarks>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="seatId">The ID of the specific seat.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The price of the seat for the event.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pricing data is invalid or not found.</exception>
        Task<decimal> GetSeatPriceAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets basic details for a specific event.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An EventDetailsDto containing event information, or null if not found.</returns>
         /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets basic details for multiple events in a batch.
        /// </summary>
        /// <param name="eventIds">A collection of event IDs to fetch details for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A dictionary mapping found Event IDs to their details.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        Task<Dictionary<Guid, EventDetailsDto>> GetEventDetailsBatchAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default);

        // Add other methods as needed to interact with the Event Service...
    }

    /// <summary>
    /// Placeholder DTO for event details returned by the Event Service API client.
    /// Define this in a shared DTO location (e.g., Common project or Application/DTOs)
    /// based on the actual data returned by the Event Service.
    /// </summary>
    public class EventDetailsDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        // Add other relevant event properties like VenueId, Description, etc.
    }
}