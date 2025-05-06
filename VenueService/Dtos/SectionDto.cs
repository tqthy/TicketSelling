namespace VenueService.Dtos;

public class SectionDto
{
    public Guid SectionId { get; set; }
    public Guid VenueId { get; set; } // Include VenueId for context
    public string Name { get; set; }
    public int Capacity { get; set; }

    // Optionally include seats in the response. Can make responses large.
    // Decide if this is needed or if seats are always fetched separately.
    // For the Create response, returning them might be useful.
    public List<SeatDto>? Seats { get; set; } = new List<SeatDto>(); // Initialize
}