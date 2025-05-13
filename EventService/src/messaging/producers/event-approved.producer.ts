import { Injectable, Inject } from "@nestjs/common";
import { ClientProxy } from "@nestjs/microservices";
import { EventApproved } from "../interfaces/message-types.interface";
import { LoggerService } from "../../common/services/logger.service";
import { firstValueFrom } from "rxjs";

@Injectable()
export class EventApprovedProducer {
  private readonly logger = new LoggerService(EventApprovedProducer.name);

  constructor(
    @Inject("RABBITMQ_CLIENT") private readonly client: ClientProxy
  ) {}

  async publishEventApproved(
    eventId: string,
    venueId: string,
    seatIds: string[]
  ): Promise<void> {
    const message: EventApproved = {
      eventId,
      venueId,
      seatIds,
      timestamp: new Date(),
    };

    this.logger.log(
      `Publishing EventApproved message for event ${eventId} with ${seatIds.length} seats`
    );

    try {
      await firstValueFrom(this.client.emit("event.approved", message));
    } catch (error) {
      throw error;
    }
  }
}
