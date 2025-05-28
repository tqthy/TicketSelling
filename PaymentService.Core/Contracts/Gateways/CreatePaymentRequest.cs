namespace PaymentService.Core.Contracts.Gateways;

public class CreatePaymentRequest
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string PaymentGateway { get; set; }
    
    public string OrderType { get; set; }
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; }
    public string IpAddress { get; set; }
    public string Currency { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
}
    