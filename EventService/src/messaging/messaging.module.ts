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
              `amqp://${configService.get("RABBITMQ_USER", "user")}:${configService.get(
                "RABBITMQ_PASSWORD",
                "user123"
              )}@${configService.get("RABBITMQ_HOST", "rabbitmq")}:${configService.get(
                "RABBITMQ_PORT",
                "5672"
              )}`,
            ],
            queue: "event_service_queue",
            queueOptions: {
              durable: true,
            },
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
