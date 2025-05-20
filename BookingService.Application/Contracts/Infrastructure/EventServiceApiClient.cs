namespace BookingService.Application.Contracts.Infrastructure;

public class EventServiceApiClient : IEventServiceApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://eventservice/";
    
    public EventServiceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }
    public Task<decimal> GetSeatPriceAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<Guid, EventDetailsDto>> GetEventDetailsBatchAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}