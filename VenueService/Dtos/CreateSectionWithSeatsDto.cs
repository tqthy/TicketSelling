using System.ComponentModel.DataAnnotations;

namespace VenueService.Dtos;

public class CreateSectionWithSeatsDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } // Name of the section

    // Optional list of seats to create along with the section
    // If null or empty, only the section is created.
    public List<CreateSeatDto>? Seats { get; set; }

    // Capacity might be omitted if seats are provided, or used as a validation check
    public int? Capacity { get; set; } // Optional: Could be calculated or stored
}