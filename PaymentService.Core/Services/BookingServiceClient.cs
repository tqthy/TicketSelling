using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PaymentService.Core.Contracts;

namespace PaymentService.Core.Services
{
    public class BookingServiceClient : IBookingServiceClient
    {
        private readonly HttpClient _httpClient;

        public BookingServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateBookingStatusAsync(Guid bookingId, string status)
        {
            // _httpClient.DefaultRequestHeaders.Add("X-User-Id", "some-user-id"); 
            // _httpClient.DefaultRequestHeaders.Add("X-User-Roles", "Admin,User,Organizer");
            var payload = new { Status = status };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync($"/api/bookings/{bookingId}/status", content);
            response.EnsureSuccessStatusCode();
        }
    }
}