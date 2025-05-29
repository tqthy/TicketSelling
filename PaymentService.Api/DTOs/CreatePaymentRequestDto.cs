namespace PaymentService.Api.DTOs;

public class CreatePaymentRequestDto
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string Cvv { get; set; }
}