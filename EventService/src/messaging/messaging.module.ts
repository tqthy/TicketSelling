import { Module } from "@nestjs/common";
import { ConfigModule } from "@nestjs/config";
import { EventApprovedProducer } from "./producers/event-approved.producer";

@Module({
  imports: [ConfigModule],
  providers: [EventApprovedProducer],
  exports: [EventApprovedProducer],
})
export class MessagingModule {}
