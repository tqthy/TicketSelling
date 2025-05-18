import { Module } from "@nestjs/common";
import { ClientsModule, Transport } from "@nestjs/microservices";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { EventApprovedProducer } from "./producers/event-approved.producer";

@Module({
  imports: [
    ClientsModule.registerAsync([
      {
        name: "RABBITMQ_CLIENT", 
        imports: [ConfigModule], 
        useFactory: (configService: ConfigService) => ({
          transport: Transport.RMQ,
          options: {
            urls: [
              `amqp://${configService.get(
                  "RABBITMQ_USER",
                  "guest"
              )}:${configService.get(
                  "RABBITMQ_PASSWORD",
                  "guest"
              )}@${configService.get("RABBITMQ_HOST", "localhost")}:5672`,
            ],
            exchange: "event-approved", 
            exchangeType: "fanout",
          },
        }),
        inject: [ConfigService],
      },
    ]),
  ],
  providers: [EventApprovedProducer],
  exports: [EventApprovedProducer],
})
export class MessagingModule {}