namespace PaymentService.Core.Contracts;

public interface IBookingServiceClient
{
    Task UpdateBookingStatusAsync(Guid bookingId, string status);
}