using System.Net.Http.Json;

namespace BookingService.Application.Contracts.Infrastructure;

public class VenueServiceApiClient : IVenueServiceApiClient
{
    private readonly HttpClient _httpClient;

    public VenueServiceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<SeatDetailsDto?> GetSeatDetailsAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<Guid, SeatDetailsDto>> GetSeatDetailsBatchAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        if (seatIds == null || !seatIds.Any())
        {
            throw new ArgumentException("Seat IDs cannot be null or empty.", nameof(seatIds));
        }

        const int MaxBatchSize = 100;
        var seatDetailsDictionary = new Dictionary<Guid, SeatDetailsDto>();

        // Split the seatIds into batches of 100
        var seatIdBatches = seatIds
            .Select((id, index) => new { id, index })
            .GroupBy(x => x.index / MaxBatchSize)
            .Select(group => group.Select(x => x.id).ToList());

        foreach (var batch in seatIdBatches)
        {
            var request = new { SeatIds = batch };
            var response = await _httpClient.PostAsJsonAsync("api/venues/seats/batch-details", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var seatDetails = await response.Content.ReadFromJsonAsync<List<SeatDetailsDto>>(cancellationToken: cancellationToken);
                if (seatDetails != null)
                {
                    foreach (var detail in seatDetails)
                    {
                        seatDetailsDictionary[detail.SeatId] = detail;
                    }
                }
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }

        return seatDetailsDictionary;
    }

    public Task<Guid?> GetSectionIdForSeatAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<SeatDetailsDto>> GetSeatsForVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
