import { Injectable, OnModuleInit } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import {
  EventApproved,
  SeatWithPrice,
} from "../interfaces/message-types.interface";
import { LoggerService } from "../../common/services/logger.service";
import * as amqp from "amqplib";

@Injectable()
export class EventApprovedProducer implements OnModuleInit {
  private readonly logger = new LoggerService(EventApprovedProducer.name);
  private connection: any;
  private channel: any;

  private readonly rabbitmqUser: string;
  private readonly rabbitmqPassword: string;
  private readonly rabbitmqHost: string;
  private readonly rabbitmqPort: number;
  private readonly exchangeName: string;
  private readonly exchangeType: string;
  private readonly queueName: string;
  private readonly routingKey: string;

  constructor(private readonly configService: ConfigService) {
    this.rabbitmqUser = this.configService.get("RABBITMQ_USER", "guest");
    this.rabbitmqPassword = this.configService.get(
      "RABBITMQ_PASSWORD",
      "guest"
    );
    this.rabbitmqHost = this.configService.get("RABBITMQ_HOST", "rabbitmq");
    this.rabbitmqPort = parseInt(
      this.configService.get("RABBITMQ_PORT", "5672")
    );
    this.exchangeName = "event-approved";
    this.exchangeType = "fanout";
    this.queueName = "event_service_queue";
    this.routingKey = "event.approved";
    this.logger.log("RabbitMQ configuration loaded from environment variables");
  }

  async onModuleInit() {
    try {
      const url = `amqp://${this.rabbitmqUser}:${this.rabbitmqPassword}@${this.rabbitmqHost}:${this.rabbitmqPort}`;
      this.logger.log(`Connecting to RabbitMQ at: ${url}`);

      this.connection = await amqp.connect(url);
      this.channel = await this.connection.createChannel();

      await this.channel.assertExchange(this.exchangeName, this.exchangeType, {
        durable: true,
      });

      await this.channel.assertQueue(this.queueName, { durable: true });

      // bind queue to the exchange (with fanout, no need for routing key)
      await this.channel.bindQueue(
        this.queueName,
        this.exchangeName,
        this.routingKey
      );

      this.logger.log(
        `Successfully connected to RabbitMQ and set up exchange ${this.exchangeName} with queue ${this.queueName}`
      );
    } catch (error) {
      this.logger.error(
        `Failed to initialize RabbitMQ connection: ${error.message}`,
        error
      );
    }
  }

  async publishEventApproved(
    eventId: string,
    venueId: string,
    seats: SeatWithPrice[]
  ): Promise<void> {
    const message: EventApproved = {
      eventId,
      venueId,
      seats,
      timestamp: new Date(),
    };

    this.logger.log(
      `Publishing EventApproved message for event ${eventId} with ${seats.length} seats`
    );

    try {
      if (!this.channel) {
        this.logger.log(
          "RabbitMQ channel not initialized. Attempting to reconnect..."
        );
        await this.onModuleInit();

        if (!this.channel) {
          throw new Error("Failed to establish RabbitMQ connection");
        }
      }

      const messageBuffer = Buffer.from(JSON.stringify(message));
      const published = this.channel.publish(
        this.exchangeName,
        this.routingKey,
        messageBuffer,
        { persistent: true }
      );

      if (published) {
        this.logger.log(
          `Successfully published message to exchange ${this.exchangeName} with routing key ${this.routingKey}`
        );
      } else {
        throw new Error("Failed to publish message to RabbitMQ");
      }
    } catch (error) {
      this.logger.error(`Error publishing message: ${error.message}`, error);
      throw error;
    }
  }
}
