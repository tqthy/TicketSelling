using System.ComponentModel.DataAnnotations;

namespace VenueService.Dtos;

public class UpdateSectionDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } // Name of the section
    
    public int? Capacity { get; set; }
}