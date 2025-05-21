using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VenueService.Dtos; 
using VenueService.Services.Interfaces; 

namespace VenueService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService; 
        private readonly ILogger<VenuesController> _logger;

        // Inject IVenueService via constructor
        public VenuesController(IVenueService venueService, ILogger<VenuesController> logger)
        {
            _venueService = venueService;
            _logger = logger;
        }

        // GET: api/venues
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VenueDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenuesAsync()
        {
            _logger.LogInformation("Getting all Venues");
            var venues = await _venueService.GetAllVenuesAsync();
            return Ok(venues); // Simply return the result from the service
        }

        

        // POST: api/venues
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(VenueDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Still needed for model validation
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<VenueDto>> CreateVenueAsync(CreateVenueDto createVenueDto)
        {
             // Model validation still happens before the controller action is called
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdVenueDto = await _venueService.CreateVenueAsync(createVenueDto);

            var createdVenueRouteName = nameof(GetVenueByIdAsync); 
            // Original return statement
            return CreatedAtAction(createdVenueRouteName, new { id = createdVenueDto.VenueId }, createdVenueDto);
        }

        // GET: api/venues/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VenueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VenueDto>> GetVenueByIdAsync(Guid id)
        {
            var venueDto = await _venueService.GetVenueByIdAsync(id);

            if (venueDto == null)
            {
                return NotFound($"Venue with ID {id} not found."); // Handle null from service
            }

            return Ok(venueDto);
        }
        
        // PUT: api/venues/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For model validation & potentially ID mismatch
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // If handling concurrency here
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateVenueAsync(Guid id, UpdateVenueDto updateVenueDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Optional: Check if ID in route matches DTO if it contains an ID
            // if (id != updateVenueDto.Id) { return BadRequest("ID mismatch"); }

            try
            {
                var success = await _venueService.UpdateVenueAsync(id, updateVenueDto);

                if (!success)
                {
                    // Service returned false, meaning the venue was not found
                    return NotFound($"Venue with ID {id} not found.");
                }

                return NoContent(); // Return 204 on successful update
            }
            catch (DbUpdateConcurrencyException ex) // Catch specific exception if re-thrown by service
            {
                 // Log the exception ex
                 return Conflict("The venue was modified by another user. Please refresh and try again.");
            }
            // Can add catches for other custom exceptions from the service layer if needed
        }

        // DELETE: api/venues/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteVenueAsync(Guid id)
        {
            var success = await _venueService.DeleteVenueAsync(id);

            if (!success)
            {
                // Service returned false, meaning the venue was not found
                return NotFound($"Venue with ID {id} not found.");
            }

            return NoContent(); // Return 204 on successful deletion
        }

        [HttpGet("{venueId}/seats/")]
        [ProducesResponseType(typeof(IEnumerable<SeatDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetSeatsForVenueAsync(Guid venueId)
        {
            try
            {
                var seats = await _venueService.GetSeatsForVenueAsync(venueId);

                if (!seats.Any())
                {
                    return NotFound($"No seats found for venue with ID {venueId}.");
                }
                return Ok(seats);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching seats for venue {VenueId}.", venueId);
                if (ex.Message == "Venue not found.")
                {
                    return NotFound($"Venue with ID {venueId} not found.");
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

    }
}