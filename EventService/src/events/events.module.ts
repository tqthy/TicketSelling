import { Module } from "@nestjs/common";
import { TypeOrmModule } from "@nestjs/typeorm";
import { EventsService } from "./events.service";
import { EventsController } from "./events.controller";
import { Event } from "./entities/event.entity";
import { EventSectionPricing } from "./entities/event-section-pricing.entity";
import { EventOwnerGuard } from "./guards/event-owner.guard";
import { MessagingModule } from "../messaging/messaging.module";
import { VenueModule } from "../venue/venue.module";

@Module({
  imports: [
    TypeOrmModule.forFeature([Event, EventSectionPricing]),
    MessagingModule,
    VenueModule,
  ],
  controllers: [EventsController],
  providers: [EventsService, EventOwnerGuard],
  exports: [EventsService],
})
export class EventsModule {}
