export interface EventApproved {
  eventId: string;
  venueId: string;
  seats: SeatWithPrice[];
  timestamp: Date;
}

export interface SeatWithPrice {
  seatId: string;
  price: number;
}
