namespace VenueService.Dtos;

public class SeatDto
{
    public Guid SeatId { get; set; }
    public Guid SectionId { get; set; } // Include SectionId for context
    public string SeatNumber { get; set; }
    public string RowNumber { get; set; }
    public int? SeatInRow { get; set; }
}