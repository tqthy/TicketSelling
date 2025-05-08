using Microsoft.AspNetCore.Mvc;
using VenueService.Dtos;
using VenueService.Services.Interfaces;

namespace VenueService.Controllers;

[ApiController]
[Route("api/venues/seats")] 
public class SeatsController : ControllerBase
{
    private readonly SeatService _seatService; // Example service in VenueService
    private readonly ILogger<SeatsController> _logger;
    private const int MaxBatchSize = 100; // Example limit

    public SeatsController(SeatService seatService, ILogger<SeatsController> logger)
    {
        _seatService = seatService;
        _logger = logger;
    }

    // Define a request model if using Option B for the request body
    public class BatchSeatDetailsRequest
    {
        public List<Guid> SeatIds { get; set; }
    }

    [HttpPost("batch-details")] 
    [ProducesResponseType(typeof(List<SeatDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSeatDetailsBatch([FromBody] BatchSeatDetailsRequest request)
    {
        if (request == null || request.SeatIds == null || !request.SeatIds.Any())
        {
            return BadRequest("A list of seat IDs must be provided.");
        }

        if (request.SeatIds.Count > MaxBatchSize)
        {
            return BadRequest($"The number of requested seat IDs exceeds the maximum batch size of {MaxBatchSize}.");
        }
        
        _logger.LogInformation("Received request for batch seat details. Count: {Count}", request.SeatIds.Count);

        try
        {
            // The _seatFinderService.GetDetailsForManySeatsAsync would contain the logic
            // to query the database for the provided seat IDs.
            var seatDetails = await _seatService.GetDetailsForManySeatsAsync(request.SeatIds);
            
            // seatDetails would be IEnumerable<VenueService.Api.Models.SeatDetailsResponse>
            // It's important that VenueService only returns details for seats that actually exist.
            // BookingService will then build its Dictionary<Guid, BookingService.Application.Contracts.Infrastructure.SeatDetailsDto>
            // from this list.

            return Ok(seatDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching batch seat details.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}