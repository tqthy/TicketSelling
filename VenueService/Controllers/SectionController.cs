// File: Controllers/SectionsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VenueService.Dtos;
using VenueService.Services.Interfaces; // Your service interface

namespace VenueService.Controllers
{
    // This route ensures all actions in this controller are nested under a specific venue
    [Route("api/venues/{venueId}/sections")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly IVenueService _venueService; // Or ISectionService if you created one
        private readonly ILogger<SectionController> _logger;

        public SectionController(IVenueService venueService, ILogger<SectionController> logger)
        {
            _venueService = venueService;
            _logger = logger;
        }

        // POST /api/venues/{venueId}/sections
        /// <summary>
        /// Creates a new section, optionally with seats, for a specific venue.
        /// Requires Admin or Organizer role.
        /// </summary>
        /// <param name="venueId">The ID of the venue.</param>
        /// <param name="sectionWithSeatsDto">The section details and optional seat list.</param>
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(SectionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // If venueId doesn't exist
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SectionDto>> CreateSectionWithSeatsAsync(Guid venueId, [FromBody] CreateSectionWithSeatsDto sectionWithSeatsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Call the service method
                var createdSectionDto = await _venueService.CreateSectionWithSeatsAsync(venueId, sectionWithSeatsDto);

                // Check if service indicated venue not found
                if (createdSectionDto == null)
                {
                    return NotFound($"Venue with ID {venueId} not found.");
                }
                
                return CreatedAtAction(nameof(GetSectionByIdAsync), 
                    new { venueId = venueId, sectionId = createdSectionDto.SectionId }, 
                    createdSectionDto);
            }
            catch (Exception ex)
            {
                // Log the exception details from the service layer failure
                _logger.LogError(ex, "An error occurred while creating section for venue {VenueId}.", venueId);
                // Return a generic server error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while creating the section.");
            }
        }

        // GET /api/venues/{venueId}/sections/{sectionId}
        /// <summary>
        /// Gets a specific section by ID for a specific venue.
        /// </summary>
        /// <param name="venueId">The ID of the venue.</param>
        /// <param name="sectionId">The ID of the section.</param>
        [HttpGet("{sectionId}", Name = "GetSectionById")] // Added Name for potential CreatedAtRoute use
        [ProducesResponseType(typeof(SectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SectionDto>> GetSectionByIdAsync(Guid venueId, Guid sectionId)
        {
             // Assuming GetSectionByIdAsync exists in the service layer
            var sectionDto = await _venueService.GetSectionByIdAsync(venueId, sectionId);

            if (sectionDto == null)
            {
                return NotFound($"Section with ID {sectionId} not found in Venue {venueId}.");
            }

            return Ok(sectionDto);
        }
        

        // GET /api/venues/{venueId}/sections
        /// <summary>
        /// Gets all sections for a specific venue.
        /// </summary>
        /// <param name="venueId">The ID of the venue.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SectionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // If venue itself doesn't exist
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetAllSectionsForVenueAsync(Guid venueId)
        {
            // You might want a service method that specifically checks if the venue exists first,
            // or let GetSectionsByVenueAsync handle it (e.g., by returning empty or specific result).
            // For now, assuming service method handles/returns empty if venue not found.
            var sections = await _venueService.GetAllSectionsForVenueAsync(venueId);
            // If GetSectionsByVenueAsync returns empty because venue not found, this still returns 200 OK with empty array.
            // You might want explicit 404 if venue doesn't exist.
            // The service method currently returns empty list for non-existent venue.
            // If you modify service to throw VenueNotFoundException, you'd catch it here.
            return Ok(sections);
        }

        // PUT /api/venues/{venueId}/sections/{sectionId}
        /// <summary>
        /// Updates a specific section. Requires Admin or Organizer role.
        /// </summary>
        /// <param name="venueId">The ID of the venue.</param>
        /// <param name="sectionId">The ID of the section to update.</param>
        /// <param name="updateSectionDto">The data to update the section with.</param>
        [HttpPut("{sectionId}")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(SectionDto), StatusCodes.Status200OK)] // Or 204 NoContent
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SectionDto>> UpdateSectionAsync(Guid venueId, Guid sectionId, [FromBody] UpdateSectionDto updateSectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedSectionDto = await _venueService.UpdateSectionAsync(venueId, sectionId, updateSectionDto);

                if (updatedSectionDto == null)
                {
                    return NotFound($"Section with ID {sectionId} not found in Venue {venueId}, or venue itself not found.");
                }
                return Ok(updatedSectionDto); // Return 200 OK with updated resource
                // Alternatively, return NoContent(); for PUT if you prefer not to return the body.
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating section {SectionId} for venue {VenueId}.", sectionId, venueId);
                return Conflict("The section was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error updating section {SectionId} for venue {VenueId}.", sectionId, venueId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // DELETE /api/venues/{venueId}/sections/{sectionId}
        /// <summary>
        /// Deletes a specific section. Requires Admin or Organizer role.
        /// </summary>
        /// <param name="venueId">The ID of the venue.</param>
        /// <param name="sectionId">The ID of the section to delete.</param>
        [HttpDelete("{sectionId}")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteSectionAsync(Guid venueId, Guid sectionId)
        {
            try
            {
                var success = await _venueService.DeleteSectionAsync(venueId, sectionId);

                if (!success)
                {
                    return NotFound($"Section with ID {sectionId} not found in Venue {venueId}, or venue itself not found.");
                }

                return NoContent(); // Successful deletion
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error deleting section {SectionId} for venue {VenueId}.", sectionId, venueId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }
    }
}