using VenueService.Dtos;

namespace VenueService.Services.Interfaces;

public interface ISeatService
{
    Task<List<SeatDetailsResponse>> GetDetailsForManySeatsAsync(List<Guid> seatIds);
}
