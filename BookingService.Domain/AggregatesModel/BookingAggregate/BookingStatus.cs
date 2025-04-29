namespace BookingService.Domain.AggregatesModel.BookingAggregate;

public static class BookingStatus
{
    public const string PendingPayment = "PendingPayment";
    public const string Confirmed = "Confirmed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    // Add other statuses as needed
}