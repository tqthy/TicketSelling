using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VenueService.Data; 
using VenueService.Dtos; 
using VenueService.Models; 
using VenueService.Services.Interfaces; 

namespace VenueService.Services 
{
    public class VenueService : IVenueService
    {
        private readonly VenueDbContext _context;
        // Optional: Inject AutoMapper or other services if needed
        // private readonly IMapper _mapper;

        public VenueService(VenueDbContext context /*, IMapper mapper*/)
        {
            _context = context;
            // _mapper = mapper;
        }

        public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
        {
            // Use Select for projection directly in the query
            return await _context.Venues
                .AsNoTracking() // Read-only query, improves performance
                .Select(v => new VenueDto // Manual Mapping (or use AutoMapper)
                {
                    VenueId = v.VenueId,
                    Name = v.Name,
                    Address = v.Address,
                    City = v.City,
                    OwnerUserId = v.OwnerUserId,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<VenueDto?> GetVenueByIdAsync(Guid id)
        {
             return await _context.Venues
                .AsNoTracking() // Read-only query
                .Where(v => v.VenueId == id)
                .Select(v => new VenueDto // Manual Mapping
                {
                    VenueId = v.VenueId,
                    Name = v.Name,
                    Address = v.Address,
                    City = v.City,
                    OwnerUserId = v.OwnerUserId,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .FirstOrDefaultAsync(); // Returns null if not found
        }

        public async Task<VenueDto> CreateVenueAsync(CreateVenueDto createVenueDto)
        {
            // Manual Mapping (or use AutoMapper)
            var venue = new Venue
            {
                VenueId = Guid.NewGuid(),
                Name = createVenueDto.Name,
                Address = createVenueDto.Address,
                City = createVenueDto.City,
                OwnerUserId = createVenueDto.OwnerUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            // Map back to DTO for return (Manual Mapping)
            return new VenueDto
            {
                 VenueId = venue.VenueId,
                 Name = venue.Name,
                 Address = venue.Address,
                 City = venue.City,
                 OwnerUserId = venue.OwnerUserId,
                 CreatedAt = venue.CreatedAt,
                 UpdatedAt = venue.UpdatedAt
            };
        }

        public async Task<bool> UpdateVenueAsync(Guid id, UpdateVenueDto updateVenueDto)
        {
            var existingVenue = await _context.Venues.FindAsync(id);

            if (existingVenue == null)
            {
                return false; // Indicate not found
            }

            // Update properties (Manual Mapping or AutoMapper)
            existingVenue.Name = updateVenueDto.Name;
            existingVenue.Address = updateVenueDto.Address;
            existingVenue.City = updateVenueDto.City;
            existingVenue.OwnerUserId = updateVenueDto.OwnerUserId;
            existingVenue.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true; // Indicate success
            }
            catch (DbUpdateConcurrencyException)
            {
                // Re-throw the exception to be handled by the controller or middleware
                // Or check existence again and return false if deleted concurrently
                 if (!await VenueExists(id))
                 {
                     return false; // Return false as if it wasn't found
                 }
                 else
                 {
                      throw; // Re-throw if it still exists but had a concurrency issue
                 }
            }
        }

        public async Task<bool> DeleteVenueAsync(Guid id)
        {
            var venueToDelete = await _context.Venues.FindAsync(id);

            if (venueToDelete == null)
            {
                return false; // Indicate not found
            }

            _context.Venues.Remove(venueToDelete);
            await _context.SaveChangesAsync();
            return true; // Indicate success
        }

        // Private helper (optional, could be used in update concurrency check)
        private async Task<bool> VenueExists(Guid id)
        {
            return await _context.Venues.AnyAsync(e => e.VenueId == id);
        }
    }
}