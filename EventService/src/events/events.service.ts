import {
  Injectable,
  NotFoundException,
  UnauthorizedException,
  InternalServerErrorException,
} from "@nestjs/common";
import { LoggerService } from "../common/services/logger.service";
import { InjectRepository } from "@nestjs/typeorm";
import { Repository } from "typeorm";
import { Event, EventStatus } from "./entities/event.entity";
import { EventSectionPricing } from "./entities/event-section-pricing.entity";
import { CreateEventDto } from "./dto/create-event.dto";
import { UpdateEventDto } from "./dto/update-event.dto";
import { RescheduleEventDto } from "./dto/reschedule-event.dto";
import { EventApprovedProducer } from "../messaging/producers/event-approved.producer";
import { VenueService } from "../venue/venue.service";

@Injectable()
export class EventsService {
  private readonly logger = new LoggerService(EventsService.name);
  constructor(
    @InjectRepository(Event)
    private readonly eventRepository: Repository<Event>,
    @InjectRepository(EventSectionPricing)
    private readonly eventSectionPricingRepository: Repository<EventSectionPricing>,
    private readonly eventApprovedProducer: EventApprovedProducer,
    private readonly venueService: VenueService
  ) {}

  async create(
    createEventDto: CreateEventDto,
    organizerId: string
  ): Promise<Event> {
    this.logger.log(`Creating new event for organizer: ${organizerId}`);
    const { sectionPricing, startDateTime, endDateTime, ...eventData } =
      createEventDto;

    const event = this.eventRepository.create({
      ...eventData,
      organizerUserId: organizerId,
      status: EventStatus.DRAFT,
      startDateTime: new Date(startDateTime),
      endDateTime: new Date(endDateTime),
    });

    const savedEvent = await this.eventRepository.save(event);

    const pricingEntities = sectionPricing.map((pricing) => {
      return this.eventSectionPricingRepository.create({
        eventId: savedEvent.eventId,
        sectionId: pricing.sectionId,
        price: pricing.price,
      });
    });

    await this.eventSectionPricingRepository.save(pricingEntities);

    return savedEvent;
  }

  async findAll(): Promise<Event[]> {
    return this.eventRepository.find({
      relations: ["sectionPricing"],
    });
  }

  async findAllPublished(): Promise<Event[]> {
    return this.eventRepository.find({
      where: { status: EventStatus.PUBLISHED },
      relations: ["sectionPricing"],
    });
  }

  async findOne(id: string): Promise<Event> {
    this.logger.log(`Finding event with ID: ${id}`);
    const event = await this.eventRepository.findOne({
      where: { eventId: id },
      relations: ["sectionPricing"],
    });

    if (!event) {
      this.logger.error(`Event with ID ${id} not found`);
      throw new NotFoundException(`Event with ID ${id} not found`);
    }

    return event;
  }

  async update(id: string, updateEventDto: UpdateEventDto): Promise<Event> {
    const event = await this.findOne(id);

    const { ...eventData } = updateEventDto;
    Object.assign(event, eventData);
    const updatedEvent = await this.eventRepository.save(event);
    return updatedEvent;
  }

  async remove(id: string): Promise<void> {
    const event = await this.findOne(id);

    await this.eventSectionPricingRepository.delete({ eventId: id });
    await this.eventRepository.remove(event);
  }

  async approveEvent(id: string): Promise<Event> {
    const event = await this.findOne(id);

    if (event.status !== EventStatus.SUBMIT_FOR_APPROVAL) {
      throw new UnauthorizedException(
        `Cannot approve and publish event. Current status: ${event.status}. Only events in Submit for Approval status can be approved and published.`
      );
    }

    const now = new Date();
    if (event.startDateTime <= now) {
      throw new UnauthorizedException(
        "Cannot publish event. The event start date must be in the future."
      );
    }

    // event.status = EventStatus.PUBLISHED;
    const savedEvent = await this.eventRepository.save(event);

    try {
      // const seats = await this.venueService.getAllSeatsByVenue(event.venueId);

      await this.eventApprovedProducer.publishEventApproved(
        event.eventId,
        event.venueId,
        []
      );

      this.logger.log(
        `Published EventApproved message for event ${event.eventId}`
      );
    } catch (error) {
      this.logger.error(
        `Failed to publish EventApproved message for event ${event.eventId}`,
        error
      );
      throw new InternalServerErrorException(
        `Failed to publish EventApproved message: ${error.message}`
      );
    }

    return savedEvent;
  }

  async submitForApproval(id: string): Promise<Event> {
    const event = await this.findOne(id);

    if (event.status !== EventStatus.DRAFT) {
      throw new UnauthorizedException(
        `Cannot submit event for approval. Current status: ${event.status}. Only events in Draft status can be submitted for approval.`
      );
    }

    if (!this.validateEventForSubmission(event)) {
      throw new UnauthorizedException(
        "Event is missing required information and cannot be submitted for approval"
      );
    }

    event.status = EventStatus.SUBMIT_FOR_APPROVAL;
    return this.eventRepository.save(event);
  }

  async cancelEvent(id: string): Promise<Event> {
    const event = await this.findOne(id);

    if (event.status !== EventStatus.PUBLISHED) {
      throw new UnauthorizedException(
        `Cannot cancel event. Current status: ${event.status}. Only events in Published status can be canceled.`
      );
    }

    event.status = EventStatus.CANCELED;
    return this.eventRepository.save(event);
  }

  async postponeEvent(id: string): Promise<Event> {
    const event = await this.findOne(id);
    if (event.status !== EventStatus.PUBLISHED) {
      throw new UnauthorizedException(
        `Cannot postpone event. Current status: ${event.status}. Only events in Published status can be postponed.`
      );
    }

    event.status = EventStatus.POSTPONED;
    return this.eventRepository.save(event);
  }

  async rescheduleEvent(
    id: string,
    rescheduleEventDto: RescheduleEventDto
  ): Promise<Event> {
    const event = await this.findOne(id);

    if (event.status !== EventStatus.POSTPONED) {
      throw new UnauthorizedException(
        `Cannot reschedule event. Current status: ${event.status}. Only events in Postponed status can be rescheduled.`
      );
    }

    const { newStartDateTime, newEndDateTime } = rescheduleEventDto;
    const startDateTime = new Date(newStartDateTime);
    const endDateTime = new Date(newEndDateTime);
    event.startDateTime = startDateTime;
    event.endDateTime = endDateTime;
    event.status = EventStatus.RESCHEDULED;

    return this.eventRepository.save(event);
  }

  private validateEventForSubmission(event: Event): boolean {
    this.logger.log(`Validating event ${event.eventId} for submission`);

    if (
      !event.name ||
      !event.description ||
      !event.category ||
      !event.startDateTime ||
      !event.endDateTime ||
      !event.venueId ||
      !event.venueName ||
      !event.venueAddress ||
      !event.poster
    ) {
      this.logger.warn(
        `Event ${event.eventId} is missing required fields for submission`
      );
      return false;
    }

    const hasSectionPricing =
      event.sectionPricing && event.sectionPricing.length > 0;

    if (!hasSectionPricing) {
      this.logger.warn(
        `Event ${event.eventId} has no section pricing for submission`
      );
    }

    return hasSectionPricing;
  }
}
