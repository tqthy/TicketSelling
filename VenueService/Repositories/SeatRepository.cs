using Microsoft.EntityFrameworkCore;
using VenueService.Data;
using VenueService.Models;
namespace VenueService.Repositories;


public class SeatRepository : ISeatRepository
{
    private readonly VenueDbContext _context;

    public SeatRepository(VenueDbContext context)
    {
        _context = context;
    }

    // ...existing code...

    public async Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds)
    {
        return await _context.Seats
            .Include(seat => seat.Section)
            .Where(seat => seatIds.Contains(seat.SeatId))
            .ToListAsync();
    }
}
