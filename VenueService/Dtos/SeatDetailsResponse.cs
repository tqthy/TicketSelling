namespace VenueService.Dtos;

public class SeatDetailsResponse
{
    public Guid SeatId { get; set; }
    public string SeatNumber { get; set; }
    public string Row { get; set; }
    public Guid SectionId { get; set; }
    public string SectionName { get; set; }
    public Guid VenueId { get; set; }
    // Potentially other properties specific to VenueService's domain
}