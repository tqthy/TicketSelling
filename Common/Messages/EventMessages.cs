namespace Common.Messages;

public record EventApproved(
    Guid EventId,
    Guid VenueId,
    List<SeatWithPrice> Seats,
    DateTime Timestamp
);

public record SeatWithPrice(
    Guid SeatId,
    decimal Price
);