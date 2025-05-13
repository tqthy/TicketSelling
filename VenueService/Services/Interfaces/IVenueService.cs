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
        
        /// <summary>
        /// Creates a new section, optionally with its seats, within a specified venue.
        /// </summary>
        /// <param name="venueId">The ID of the venue to add the section to.</param>
        /// <param name="sectionWithSeatsDto">The DTO containing section details and optional seat definitions.</param>
        /// <returns>The created SectionDto, potentially including created seats, or null if the venue was not found.</returns>
        Task<SectionDto?> CreateSectionWithSeatsAsync(Guid venueId, CreateSectionWithSeatsDto sectionWithSeatsDto);

        // Add other Section/Seat specific methods here or in separate interfaces...
        Task<IEnumerable<SectionDto>> GetSectionsByVenueAsync(Guid venueId);
        Task<SectionDto?> GetSectionByIdAsync(Guid venueId, Guid sectionId);
        
        /// <summary>
        /// Retrieves all sections for a given venue.
        /// </summary>
        Task<IEnumerable<SectionDto>> GetAllSectionsForVenueAsync(Guid venueId);

        /// <summary>
        /// Updates an existing section.
        /// </summary>
        /// <param name="venueId">The ID of the venue the section belongs to.</param>
        /// <param name="sectionId">The ID of the section to update.</param>
        /// <param name="updateSectionDto">The DTO with updated section data.</param>
        /// <returns>The updated SectionDto, or null if not found or venue mismatch.</returns>
        Task<SectionDto?> UpdateSectionAsync(Guid venueId, Guid sectionId, UpdateSectionDto updateSectionDto);

        /// <summary>
        /// Deletes a section.
        /// </summary>
        /// <param name="venueId">The ID of the venue the section belongs to.</param>
        /// <param name="sectionId">The ID of the section to delete.</param>
        /// <returns>True if deletion was successful, false if not found or venue mismatch.</returns>
        Task<bool> DeleteSectionAsync(Guid venueId, Guid sectionId);

        Task<List<SeatDto>> GetSeatsForVenueAsync(Guid venueId);
    }
}