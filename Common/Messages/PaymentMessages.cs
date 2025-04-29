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