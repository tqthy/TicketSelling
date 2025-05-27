namespace VenueService.Dtos;


public class VenueDto
{
    public Guid VenueId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public Guid? OwnerUserId { get; set; }
    public List<SectionDto>? Sections { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
}