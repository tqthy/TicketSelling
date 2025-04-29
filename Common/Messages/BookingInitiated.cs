namespace Common.Messages;
public record BookingInitiated(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    Guid SeatId,
    decimal TotalPrice,
    DateTime Timestamp
);