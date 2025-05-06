using System.ComponentModel.DataAnnotations;

namespace VenueService.Dtos;

public class CreateSeatDto
{
    [StringLength(10, MinimumLength = 1)]
    public string SeatNumber { get; set; } // e.g., "A1", "101", "Box 3"

    [StringLength(5)]
    public string RowNumber { get; set; } // e.g., "A", "B", "GA"

    public int? SeatInRow { get; set; } // Optional: e.g., 1, 2 (useful for ordered rows)
}