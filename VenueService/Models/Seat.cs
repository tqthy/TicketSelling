using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueService.Models // Adjust namespace if needed
{
    [Table("Seats")] // Explicitly map to the 'Seats' table
    public class Seat
    {
        [Key] // Marks SeatId as the Primary Key
        public Guid SeatId { get; set; }

        [StringLength(10)] // Example: e.g., "A1", "B12"
        public string SeatNumber { get; set; }

        [StringLength(5)] // Example: e.g., "A", "B"
        public string RowNumber { get; set; }

        public int SeatInRow { get; set; } // Example: e.g., 1, 12

        // Foreign Key Property for the relationship to Section
        [Required]
        public Guid SectionId { get; set; }

        // Navigation Property: Link back to the Section this Seat belongs to
        [ForeignKey("SectionId")] // Explicitly links SectionId FK to the Section navigation property
        public virtual Section Section { get; set; }
    }
}