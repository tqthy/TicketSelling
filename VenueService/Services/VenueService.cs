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
        private readonly ILogger<VenueService> _logger; 

        // Optional: Inject AutoMapper or other services if needed
        // private readonly IMapper _mapper;

        public VenueService(VenueDbContext context, ILogger<VenueService> logger /*, IMapper mapper*/)
        {
            _context = context;
            _logger = logger;
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
        
        public async Task<SectionDto?> CreateSectionWithSeatsAsync(Guid venueId, CreateSectionWithSeatsDto sectionWithSeatsDto)
        {
            // 1. Validate Venue exists
            var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == venueId);
            if (!venueExists)
            {
                _logger.LogWarning("Cannot create section. Venue with ID {VenueId} not found.", venueId);
                return null; // Indicate venue not found
            }

            // 2. Create Section entity
            var section = new Section
            {
                SectionId = Guid.NewGuid(),
                VenueId = venueId,
                Name = sectionWithSeatsDto.Name,
                // Handle Capacity: Use provided value or calculate from seats?
                // If calculated, do it after adding seats. For now, use DTO value or default.
                Capacity = sectionWithSeatsDto.Capacity ?? sectionWithSeatsDto.Seats?.Count ?? 0,
                Seats = new List<Seat>() // Initialize collection
            };

            // 3. Start Transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 4. Add Section to context
                _context.Sections.Add(section);

                // 5. Add Seats (if provided)
                if (sectionWithSeatsDto.Seats != null && sectionWithSeatsDto.Seats.Any())
                {
                    foreach (var seatDto in sectionWithSeatsDto.Seats)
                    {
                        var seat = new Seat
                        {
                            SeatId = Guid.NewGuid(),
                            SectionId = section.SectionId, // Link to the new section
                            SeatNumber = seatDto.SeatNumber,
                            RowNumber = seatDto.RowNumber,
                            SeatInRow = seatDto.SeatInRow ?? 0 // Use default or handle null
                        };
                        section.Seats.Add(seat); // Add to navigation property
                        _context.Seats.Add(seat); // Add to context
                    }
                    // Optionally recalculate/validate capacity
                    if (sectionWithSeatsDto.Capacity.HasValue && sectionWithSeatsDto.Capacity != section.Seats.Count)
                    {
                         _logger.LogWarning("Provided capacity {DtoCapacity} does not match number of seats {SeatCount} for section {SectionName}.",
                            sectionWithSeatsDto.Capacity, section.Seats.Count, section.Name);
                         // Decide whether to throw error, use calculated value, or keep provided value
                         // For now, we keep the explicitly provided DTO capacity if it exists, otherwise use seat count.
                         section.Capacity = sectionWithSeatsDto.Capacity ?? section.Seats.Count;
                    }
                    else {
                        section.Capacity = section.Seats.Count;
                    }
                }

                // 6. Save Changes (Section and all Seats)
                await _context.SaveChangesAsync();

                // 7. Commit Transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully created Section {SectionId} with {SeatCount} seats for Venue {VenueId}",
                    section.SectionId, section.Seats.Count, venueId);

                // 8. Map to DTO for response (Manual Mapping Example)
                // Decide whether to include seats in the response DTO
                return MapSectionToDto(section, includeSeats: true); // Pass flag to include seats

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section with seats for Venue {VenueId}. Rolling back transaction.", venueId);
                await transaction.RollbackAsync();
                // Re-throw or handle appropriately - maybe return null or a specific error result
                throw; // Re-throwing for now, controller or middleware should handle
            }
        }

        // Implement other methods like GetSectionsByVenueAsync, GetSectionByIdAsync etc.
        public async Task<IEnumerable<SectionDto>> GetSectionsByVenueAsync(Guid venueId)
        {
            // Example implementation
            return await _context.Sections
                .Where(s => s.VenueId == venueId)
                .Select(s => MapSectionToDto(s, false)) // Map without seats for list view
                .ToListAsync();
        }

         public async Task<SectionDto?> GetSectionByIdAsync(Guid venueId, Guid sectionId)
         {
             // Example implementation
             var section = await _context.Sections
                .Include(s => s.Seats) // Eager load seats for detail view
                .FirstOrDefaultAsync(s => s.VenueId == venueId && s.SectionId == sectionId);

             return section == null ? null : MapSectionToDto(section, true); // Map with seats
         }

        public async Task<IEnumerable<SectionDto>> GetAllSectionsForVenueAsync(Guid venueId)
        {
            var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == venueId);
            if (!venueExists)
            {
                _logger.LogWarning("Venue with ID {VenueId} not found when trying to get sections.", venueId);
                return Enumerable.Empty<SectionDto>(); // Or throw VenueNotFoundException
            }

            return await _context.Sections
                .Where(s => s.VenueId == venueId)
                .Select(s => MapSectionToDto(s, false)) 
                .ToListAsync();
        }

        public async Task<SectionDto?> UpdateSectionAsync(Guid venueId, Guid sectionId, UpdateSectionDto updateSectionDto)
        {
            var section = await _context.Sections
                                        .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.VenueId == venueId);

            if (section == null)
            {
                _logger.LogWarning("Section with ID {SectionId} not found in Venue {VenueId} for update.", sectionId, venueId);
                return null; // Not found
            }

            // Update properties
            section.Name = updateSectionDto.Name;
            if (updateSectionDto.Capacity.HasValue)
            {
                section.Capacity = updateSectionDto.Capacity.Value;
            }
            // Note: Managing seats (adding/removing/changing count) would likely be a separate, more complex operation.
            // This PUT is primarily for section metadata.

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated Section {SectionId} in Venue {VenueId}.", sectionId, venueId);
                return MapSectionToDto(section, includeSeats: false); // Return updated section (without seats for brevity)
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating Section {SectionId}.", sectionId);
                throw; // Re-throw for controller to handle as 409 Conflict or similar
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Section {SectionId}.", sectionId);
                throw;
            }
        }

        public async Task<bool> DeleteSectionAsync(Guid venueId, Guid sectionId)
        {
            var section = await _context.Sections
                                        .Include(s => s.Seats) // Include seats if you need to handle their deletion explicitly or log count
                                        .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.VenueId == venueId);

            if (section == null)
            {
                _logger.LogWarning("Section with ID {SectionId} not found in Venue {VenueId} for deletion.", sectionId, venueId);
                return false; // Not found
            }

            // EF Core by convention should handle cascading delete for seats if the relationship is set up correctly (Section has ICollection<Seat>, Seat has SectionId FK).
            // If explicit deletion of seats is needed, you'd iterate: _context.Seats.RemoveRange(section.Seats);
            _context.Sections.Remove(section);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted Section {SectionId} (and its {SeatCount} seats) from Venue {VenueId}.",
                    sectionId, section.Seats.Count, venueId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Section {SectionId}.", sectionId);
                // Depending on DB constraints (e.g., if seats are linked to other tables not set for cascade), this might fail.
                throw;
            }
        }
         
        // Helper mapping method 
        private SectionDto MapSectionToDto(Section section, bool includeSeats)
        {
             var sectionDto = new SectionDto
             {
                  SectionId = section.SectionId,
                  VenueId = section.VenueId,
                  Name = section.Name,
                  Capacity = section.Capacity,
                  Seats = includeSeats && section.Seats != null
                        ? section.Seats.Select(seat => MapSeatToDto(seat)).ToList()
                        : new List<SeatDto>() // Return empty list if not including seats
             };
             return sectionDto;
        }

        private SeatDto MapSeatToDto(Seat seat)
        {
             return new SeatDto
             {
                 SeatId = seat.SeatId,
                 SectionId = seat.SectionId,
                 SeatNumber = seat.SeatNumber,
                 RowNumber = seat.RowNumber,
                 SeatInRow = seat.SeatInRow
             };
        }
    }
}