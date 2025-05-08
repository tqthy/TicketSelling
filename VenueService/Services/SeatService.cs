using VenueService.Dtos;
using VenueService.Repositories;
using VenueService.Services.Interfaces;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;

    public SeatService(ISeatRepository seatRepository)
    {
        _seatRepository = seatRepository;
    }

    public async Task<List<SeatDetailsResponse>> GetDetailsForManySeatsAsync(List<Guid> seatIds)
    {
        var seats = await _seatRepository.GetSeatsByIdsAsync(seatIds);

        return seats.Select(seat => new SeatDetailsResponse
        {
            SeatId = seat.SeatId,
            SeatNumber = seat.SeatNumber,
            Row = seat.RowNumber,
            SectionId = seat.SectionId,
            SectionName = seat.Section.Name,
            VenueId = seat.Section.VenueId
        }).ToList();
    }
}
