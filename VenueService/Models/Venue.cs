using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueService.Models 
{
    [Table("Venues")] // Explicitly map to the 'Venues' table
    public class Venue
    {
        [Key] // Marks VenueId as the Primary Key
        public Guid VenueId { get; set; }

        [Required] // Makes Name a required field in the database
        [StringLength(100)] // Example: Sets max length
        public string Name { get; set; }

        [StringLength(255)] // Example: Sets max length
        public string Address { get; set; }

        [StringLength(100)] // Example: Sets max length
        public string City { get; set; }

        // Foreign Key to an external User Service (optional)
        // Naming convention suggests this links to a User entity elsewhere
        public Guid? OwnerUserId { get; set; } // Nullable Guid as it's optional

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Property: A Venue can contain multiple Sections
        // Initialize to prevent null reference exceptions
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}