using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueService.Models 
{
    [Table("Sections")] // Explicitly map to the 'Sections' table
    public class Section
    {
        [Key] // Marks SectionId as the Primary Key
        public Guid SectionId { get; set; }

        [Required] // Marks Name as required
        [StringLength(50)] // Example: Sets max length
        public string Name { get; set; }

        public int Capacity { get; set; } // Calculated or stored capacity

        // Foreign Key Property for the relationship to Venue
        [Required]
        public Guid VenueId { get; set; }

        // Navigation Property: Link back to the Venue this Section belongs to
        [ForeignKey("VenueId")] // Explicitly links VenueId FK to the Venue navigation property
        public virtual Venue Venue { get; set; }

        // Navigation Property: A Section can contain multiple Seats
        // Initialize to prevent null reference exceptions
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}