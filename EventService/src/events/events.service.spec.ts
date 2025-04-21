import { Test, TestingModule } from "@nestjs/testing";
import { getRepositoryToken } from "@nestjs/typeorm";
import { Repository } from "typeorm";
import { EventsService } from "./events.service";
import { Event, EventStatus } from "./entities/event.entity";
import { EventSectionPricing } from "./entities/event-section-pricing.entity";
import { CreateEventDto } from "./dto/create-event.dto";
import { NotFoundException, UnauthorizedException } from "@nestjs/common";

// Mock repository factory
const mockRepository = () => ({
  findOne: jest.fn(),
  find: jest.fn(),
  create: jest.fn(),
  save: jest.fn(),
  remove: jest.fn(),
});

describe("EventsService", () => {
  let service: EventsService;
  let eventRepository: Repository<Event>;
  let eventSectionPricingRepository: Repository<EventSectionPricing>;

  // Mock event data
  const mockEventId = "123";
  const mockOrganizerId = "organizer123";
  const mockAdminId = "admin123";
  const mockEvent = {
    eventId: mockEventId,
    name: "Test Event",
    description: "Test Description",
    category: "MATCH",
    startDateTime: new Date(Date.now() + 86400000), // tomorrow
    endDateTime: new Date(Date.now() + 172800000), // day after tomorrow
    status: EventStatus.DRAFT,
    venueId: "123",
    venueName: "Test Venue",
    venueAddress: "Test Address",
    organizerUserId: mockOrganizerId,
    poster: "http://test.com/poster.jpg",
    images: ["http://test.com/image1.jpg", "http://test.com/image2.jpg"],
    details: "Test details",
    createdAt: new Date(),
    updatedAt: new Date(),
    sectionPricing: [
      { id: "1", eventId: mockEventId, sectionId: "1", price: 100 },
    ],
  };

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      providers: [
        EventsService,
        {
          provide: getRepositoryToken(Event),
          useFactory: mockRepository,
        },
        {
          provide: getRepositoryToken(EventSectionPricing),
          useFactory: mockRepository,
        },
      ],
    }).compile();

    service = module.get<EventsService>(EventsService);
    eventRepository = module.get<Repository<Event>>(getRepositoryToken(Event));
    eventSectionPricingRepository = module.get<Repository<EventSectionPricing>>(
      getRepositoryToken(EventSectionPricing)
    );

    // Reset mocks before each test
    jest.clearAllMocks();
  });

  it("should be defined", () => {
    expect(service).toBeDefined();
  });

  describe("findOne", () => {
    it("should return an event if found", async () => {
      const mockEvent = {
        eventId: "123",
        name: "Test Event",
        status: EventStatus.DRAFT,
        poster: "http://test.com/poster.jpg",
        images: ["http://test.com/image1.jpg"],
        details: "Test details",
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      jest
        .spyOn(eventRepository, "findOne")
        .mockResolvedValue(mockEvent as Event);

      const result = await service.findOne("123");
      expect(result).toEqual(mockEvent);
      expect(eventRepository.findOne).toHaveBeenCalledWith({
        where: { eventId: "123" },
        relations: ["sectionPricing"],
      });
    });

    it("should throw NotFoundException if event not found", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue(null);

      await expect(service.findOne("123")).rejects.toThrow(NotFoundException);
    });
  });

  describe("create", () => {
    it("should create a new event", async () => {
      const createEventDto: CreateEventDto = {
        name: "Test Event",
        description: "Test Description",
        category: "MATCH",
        date: "2025-04-22",
        startTime: "19:30",
        endTime: "22:30",
        venueId: "123",
        venueName: "Test Venue",
        venueAddress: "Test Address",
        poster: "http://test.com/poster.jpg",
        images: ["http://test.com/image1.jpg", "http://test.com/image2.jpg"],
        details: "Test details",
        sectionPricing: [
          {
            sectionId: "123",
            price: 100,
          },
        ],
      };

      const mockEvent = {
        ...createEventDto,
        eventId: "123",
        status: EventStatus.DRAFT,
        organizerUserId: "organizer123",
      };

      jest.spyOn(eventRepository, "create").mockReturnValue(mockEvent as any);
      jest.spyOn(eventRepository, "save").mockResolvedValue(mockEvent as any);
      jest
        .spyOn(eventSectionPricingRepository, "create")
        .mockReturnValue({} as any);
      jest
        .spyOn(eventSectionPricingRepository, "save")
        .mockResolvedValue({} as any);

      const result = await service.create(createEventDto, "organizer123");

      expect(result).toEqual(mockEvent);
      expect(eventRepository.create).toHaveBeenCalled();
      expect(eventRepository.save).toHaveBeenCalled();
      expect(eventSectionPricingRepository.create).toHaveBeenCalled();
      expect(eventSectionPricingRepository.save).toHaveBeenCalled();
    });
  });

  describe("submitForApproval", () => {
    it("should throw UnauthorizedException if user is not an organizer", async () => {
      await expect(
        service.submitForApproval("123", "user123", "USER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if organizer is not the creator", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      await expect(
        service.submitForApproval("123", "different-organizer", "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if event is not in DRAFT status", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      await expect(
        service.submitForApproval("123", mockOrganizerId, "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should successfully submit an event for approval", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.SUBMIT_FOR_APPROVAL,
      } as Event);

      const result = await service.submitForApproval(
        "123",
        mockOrganizerId,
        "ORGANIZER"
      );

      expect(result.status).toBe(EventStatus.SUBMIT_FOR_APPROVAL);
      expect(eventRepository.save).toHaveBeenCalled();
    });
  });

  describe("approveEvent", () => {
    it("should throw UnauthorizedException if user is not an admin", async () => {
      await expect(service.approveEvent("123", "ORGANIZER")).rejects.toThrow(
        UnauthorizedException
      );
    });

    it("should throw UnauthorizedException if event is not in SUBMIT_FOR_APPROVAL status", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      await expect(service.approveEvent("123", "ADMIN")).rejects.toThrow(
        UnauthorizedException
      );
    });

    it("should successfully approve and publish an event", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.SUBMIT_FOR_APPROVAL,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      const result = await service.approveEvent("123", "ADMIN");

      expect(result.status).toBe(EventStatus.PUBLISHED);
      expect(eventRepository.save).toHaveBeenCalled();
    });
  });

  // publishEvent tests have been combined with approveEvent tests

  describe("cancelEvent", () => {
    it("should throw UnauthorizedException if user is not an admin or organizer", async () => {
      await expect(
        service.cancelEvent("123", "user123", "USER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if organizer is not the creator", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      await expect(
        service.cancelEvent("123", "different-organizer", "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if event is not in PUBLISHED status", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      await expect(
        service.cancelEvent("123", mockOrganizerId, "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should successfully cancel an event as organizer", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.CANCELED,
      } as Event);

      const result = await service.cancelEvent(
        "123",
        mockOrganizerId,
        "ORGANIZER"
      );

      expect(result.status).toBe(EventStatus.CANCELED);
      expect(eventRepository.save).toHaveBeenCalled();
    });

    it("should successfully cancel an event as admin", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.CANCELED,
      } as Event);

      const result = await service.cancelEvent("123", "admin-id", "ADMIN");

      expect(result.status).toBe(EventStatus.CANCELED);
      expect(eventRepository.save).toHaveBeenCalled();
    });
  });

  describe("postponeEvent", () => {
    it("should throw UnauthorizedException if user is not an admin or organizer", async () => {
      await expect(
        service.postponeEvent("123", "user123", "USER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if organizer is not the creator", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      await expect(
        service.postponeEvent("123", "different-organizer", "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if event is not in PUBLISHED status", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      await expect(
        service.postponeEvent("123", mockOrganizerId, "ORGANIZER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should successfully postpone an event", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.PUBLISHED,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.POSTPONED,
      } as Event);

      const result = await service.postponeEvent(
        "123",
        mockOrganizerId,
        "ORGANIZER"
      );

      expect(result.status).toBe(EventStatus.POSTPONED);
      expect(eventRepository.save).toHaveBeenCalled();
    });
  });

  describe("rescheduleEvent", () => {
    const rescheduleEventDto = {
      date: "2025-04-29",
      startTime: "19:30",
      endTime: "22:30",
    };

    it("should throw UnauthorizedException if user is not an admin or organizer", async () => {
      await expect(
        service.rescheduleEvent("123", rescheduleEventDto, "user123", "USER")
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if organizer is not the creator", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.POSTPONED,
      } as Event);

      await expect(
        service.rescheduleEvent(
          "123",
          rescheduleEventDto,
          "different-organizer",
          "ORGANIZER"
        )
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should throw UnauthorizedException if event is not in POSTPONED status", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.DRAFT,
      } as Event);

      await expect(
        service.rescheduleEvent(
          "123",
          rescheduleEventDto,
          mockOrganizerId,
          "ORGANIZER"
        )
      ).rejects.toThrow(UnauthorizedException);
    });

    it("should successfully reschedule an event", async () => {
      jest.spyOn(eventRepository, "findOne").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.POSTPONED,
      } as Event);

      jest.spyOn(eventRepository, "save").mockResolvedValue({
        ...mockEvent,
        status: EventStatus.RESCHEDULED,
        startDateTime: new Date(
          `${rescheduleEventDto.date}T${rescheduleEventDto.startTime}:00`
        ),
        endDateTime: new Date(
          `${rescheduleEventDto.date}T${rescheduleEventDto.endTime}:00`
        ),
      } as Event);

      const result = await service.rescheduleEvent(
        "123",
        rescheduleEventDto,
        mockOrganizerId,
        "ORGANIZER"
      );

      expect(result.status).toBe(EventStatus.RESCHEDULED);
      expect(result.startDateTime).toEqual(
        new Date(
          `${rescheduleEventDto.date}T${rescheduleEventDto.startTime}:00`
        )
      );
      expect(result.endDateTime).toEqual(
        new Date(`${rescheduleEventDto.date}T${rescheduleEventDto.endTime}:00`)
      );
      expect(eventRepository.save).toHaveBeenCalled();
    });
  });
});
