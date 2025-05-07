using Microsoft.EntityFrameworkCore;
using VenueService.Models; 

namespace VenueService.Data 
{
    public class VenueDbContext : DbContext
    {
        public VenueDbContext(DbContextOptions<VenueDbContext> options) : base(options) { }

        
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Seat> Seats { get; set; }

        // Optional: Override OnModelCreating for advanced configuration (Fluent API)
        // You can define relationships, constraints, indexes, seed data, etc., here
        // if you prefer it over Data Annotations in the model classes.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Recommended to call the base method

            // Example: If you wanted to define the one-to-many relationship
            // between Venue and Section using Fluent API instead of annotations:
            /*
            modelBuilder.Entity<Venue>()
                .HasMany(v => v.Sections)        // Venue has many Sections
                .WithOne(s => s.Venue)           // Section has one Venue
                .HasForeignKey(s => s.VenueId);  // The foreign key is VenueId in Section
            */

            // Add other Fluent API configurations for Section-Seat relationship
            // or specific property configurations (like indexes, default values) here if needed.

            // Example: Configure composite key if needed (not applicable here, but useful)
            // modelBuilder.Entity<YourEntity>().HasKey(c => new { c.Key1, c.Key2 });

            // Example: Add an index
            // modelBuilder.Entity<Venue>().HasIndex(v => v.Name);
        }
    }
}