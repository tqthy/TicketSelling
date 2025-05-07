namespace Common.Messages;

public record EventApproved(
    Guid EventId,
    Guid VenueId, // Assuming VenueService can provide seats based on VenueId
    List<Guid> SeatIds, // Or, if VenueService provides seats directly for an Event
    DateTime Timestamp
);

// Placeholder for seat information if needed directly from EventApproved event,
// otherwise, this will be fetched from VenueService.
public record SeatInfo(
    Guid SeatId,
    string SeatNumber,
    string Row,
    string Section
);