namespace Common.Messages;

public record PaymentSucceeded(
    Guid BookingId,
    Guid TransactionId, // ID from the payment service transaction
    DateTime Timestamp
);

public record PaymentFailed(
    Guid BookingId,
    Guid TransactionId,
    string Reason,
    DateTime Timestamp
);

public enum PaymentGatewaySelection 
{
    Default,
    Stripe,
    PayPal,
    VnPay,
    Momo
}

public record InitiatePaymentRequested
(
    Guid BookingId,
    Guid UserId,
    string OrderType, 
    decimal Amount,   
    string Currency,  
    string OrderInfo, 
    PaymentGatewaySelection PreferredGateway, 
    string IpAddress,  
    DateTime Timestamp 
);