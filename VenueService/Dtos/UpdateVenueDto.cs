using System.ComponentModel.DataAnnotations;

namespace VenueService.Dtos;

public class UpdateVenueDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(255)]
    public string Address { get; set; }

    [StringLength(100)]
    public string City { get; set; }

    public Guid? OwnerUserId { get; set; } // Allow updating owner link
}