using VenueService.Models;

// ...existing code...

namespace VenueService.Repositories;

public interface ISeatRepository
{
    Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds);
}