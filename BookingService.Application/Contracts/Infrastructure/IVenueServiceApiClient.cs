using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingService.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Interface abstracting communication with the external Venue Service API.
    /// </summary>
    public interface IVenueServiceApiClient
    {
        /// <summary>
        /// Gets detailed information for a specific seat (like number, row, section).
        /// </summary>
        /// <param name="seatId">The ID of the seat.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A SeatDetailsDto containing seat information, or null if not found.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        Task<SeatDetailsDto?> GetSeatDetailsAsync(Guid seatId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets detailed information for multiple seats in a batch.
        /// </summary>
        /// <param name="seatIds">A collection of seat IDs to fetch details for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A dictionary mapping found Seat IDs to their details.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        Task<Dictionary<Guid, SeatDetailsDto>> GetSeatDetailsBatchAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// (Optional) Gets the Section ID for a specific Seat ID.
        /// This might be needed if the Event Service requires Section ID for pricing.
        /// </summary>
        /// <param name="seatId">The ID of the seat.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The Guid ID of the Section, or null if not found or not applicable.</returns>
        /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
        Task<Guid?> GetSectionIdForSeatAsync(Guid seatId, CancellationToken cancellationToken = default);

        // Add other methods as needed to interact with the Venue Service...
        // e.g., GetVenueDetailsAsync(Guid venueId), GetSectionDetailsAsync(Guid sectionId)
    }

    /// <summary>
    /// Placeholder DTO for seat details returned by the Venue Service API client.
    /// Define this in a shared DTO location (e.g., Common project or Application/DTOs)
    /// based on the actual data returned by the Venue Service.
    /// </summary>
    public class SeatDetailsDto
    {
        public Guid SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string Row { get; set; } = string.Empty;
        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public Guid VenueId { get; set; }
        // Add other relevant seat/section/venue properties
    }
}