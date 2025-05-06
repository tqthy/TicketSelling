using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VenueService.Dtos; 

namespace VenueService.Services.Interfaces
{
    public interface IVenueService
    {
        /// <summary>
        /// Gets all venues.
        /// </summary>
        /// <returns>A list of Venue DTOs.</returns>
        Task<IEnumerable<VenueDto>> GetAllVenuesAsync();

        /// <summary>
        /// Gets a specific venue by its ID.
        /// </summary>
        /// <param name="id">The GUID ID of the venue.</param>
        /// <returns>The Venue DTO if found; otherwise, null.</returns>
        Task<VenueDto?> GetVenueByIdAsync(Guid id); // Return null if not found

        /// <summary>
        /// Creates a new venue.
        /// </summary>
        /// <param name="createVenueDto">Data for the new venue.</param>
        /// <returns>The created Venue DTO.</returns>
        Task<VenueDto> CreateVenueAsync(CreateVenueDto createVenueDto);

        /// <summary>
        /// Updates an existing venue.
        /// </summary>
        /// <param name="id">The GUID ID of the venue to update.</param>
        /// <param name="updateVenueDto">The updated venue data.</param>
        /// <returns>True if update was successful, False if venue was not found.</returns>
        /// <exception cref="DbUpdateConcurrencyException">Thrown if a concurrency conflict occurs.</exception>
        Task<bool> UpdateVenueAsync(Guid id, UpdateVenueDto updateVenueDto);

        /// <summary>
        /// Deletes a specific venue.
        /// </summary>
        /// <param name="id">The GUID ID of the venue to delete.</param>
        /// <returns>True if deletion was successful, False if venue was not found.</returns>
        Task<bool> DeleteVenueAsync(Guid id);
    }
}