import { Test, TestingModule } from "@nestjs/testing";
import { EventsController } from "./events.controller";
import { EventsService } from "./events.service";
import { Event, EventStatus } from "./entities/event.entity";
import { CreateEventDto } from "./dto/create-event.dto";
import { UpdateEventDto } from "./dto/update-event.dto";
import { RescheduleEventDto } from "./dto/reschedule-event.dto";
import { NotFoundException, UnauthorizedException } from "@nestjs/common";
import { RequestWithUser } from "../auth/interfaces/request-with-user.interface";

describe("EventsController", () => {
  let controller: EventsController;
  let service: EventsService;

  const mockEventId = "123";
  const mockOrganizerId = "organizer123";
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

  const mockRequest = {
    user: {
      userId: mockOrganizerId,
      role: "ORGANIZER",
    },
  } as RequestWithUser;

  const mockAdminRequest = {
    user: {
      userId: "admin123",
      role: "ADMIN",
    },
  } as RequestWithUser;

  const mockEventsService = {
    create: jest.fn(),
    findAll: jest.fn(),
    findAllPublished: jest.fn(),
    findOne: jest.fn(),
    update: jest.fn(),
    remove: jest.fn(),
    submitForApproval: jest.fn(),
    approveEvent: jest.fn(),
    cancelEvent: jest.fn(),
    postponeEvent: jest.fn(),
    rescheduleEvent: jest.fn(),
  };

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      controllers: [EventsController],
      providers: [
        {
          provide: EventsService,
          useValue: mockEventsService,
        },
      ],
    }).compile();

    controller = module.get<EventsController>(EventsController);
    service = module.get<EventsService>(EventsService);

    jest.clearAllMocks();
  });

  it("should be defined", () => {
    expect(controller).toBeDefined();
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

      jest.spyOn(service, "create").mockResolvedValue(mockEvent as Event);

      const result = await controller.create(createEventDto, mockRequest);

      expect(result).toEqual(mockEvent);
    });
  });

  describe("findAll", () => {
    it("should return an array of events", async () => {
      const events = [mockEvent];
      jest.spyOn(service, "findAll").mockResolvedValue(events as Event[]);

      const result = await controller.findAll();

      expect(result).toEqual(events);
    });
  });

  describe("findAllPublished", () => {
    it("should return an array of published events", async () => {
      const publishedEvents = [{ ...mockEvent, status: EventStatus.PUBLISHED }];
      jest
        .spyOn(service, "findAllPublished")
        .mockResolvedValue(publishedEvents as Event[]);

      const result = await controller.findAllPublished();

      expect(result).toEqual(publishedEvents);
    });
  });

  describe("findOne", () => {
    it("should return a single event", async () => {
      jest.spyOn(service, "findOne").mockResolvedValue(mockEvent as Event);

      const result = await controller.findOne(mockEventId);

      expect(result).toEqual(mockEvent);
    });

    it("should throw NotFoundException if event not found", async () => {
      jest.spyOn(service, "findOne").mockRejectedValue(new NotFoundException());

      await expect(controller.findOne("non-existent-id")).rejects.toThrow(
        NotFoundException
      );
    });
  });

  describe("update", () => {
    it("should update an event", async () => {
      const updateEventDto: UpdateEventDto = {
        name: "Updated Event Name",
      };

      const updatedEvent = { ...mockEvent, name: "Updated Event Name" };
      jest.spyOn(service, "update").mockResolvedValue(updatedEvent as Event);

      const result = await controller.update(
        mockEventId,
        updateEventDto,
        mockRequest
      );

      expect(result).toEqual(updatedEvent);
    });
  });

  describe("remove", () => {
    it("should remove an event", async () => {
      jest.spyOn(service, "remove").mockResolvedValue(undefined);

      await controller.remove(mockEventId, mockRequest);
    });
  });

  describe("submitForApproval", () => {
    it("should submit an event for approval", async () => {
      const submittedEvent = {
        ...mockEvent,
        status: EventStatus.SUBMIT_FOR_APPROVAL,
      };
      jest
        .spyOn(service, "submitForApproval")
        .mockResolvedValue(submittedEvent as Event);

      const result = await controller.submitForApproval(
        mockEventId,
        mockRequest
      );

      expect(result).toEqual(submittedEvent);
    });
  });

  describe("approveEvent", () => {
    it("should approve and publish an event", async () => {
      const publishedEvent = { ...mockEvent, status: EventStatus.PUBLISHED };
      jest
        .spyOn(service, "approveEvent")
        .mockResolvedValue(publishedEvent as Event);

      const result = await controller.approveEvent(
        mockEventId,
        mockAdminRequest
      );

      expect(result).toEqual(publishedEvent);
    });

    it("should throw UnauthorizedException if user is not an admin", async () => {
      jest
        .spyOn(service, "approveEvent")
        .mockRejectedValue(new UnauthorizedException());

      await expect(
        controller.approveEvent(mockEventId, mockRequest)
      ).rejects.toThrow(UnauthorizedException);
    });
  });

  describe("cancelEvent", () => {
    it("should cancel an event", async () => {
      const canceledEvent = { ...mockEvent, status: EventStatus.CANCELED };
      jest
        .spyOn(service, "cancelEvent")
        .mockResolvedValue(canceledEvent as Event);

      const result = await controller.cancelEvent(mockEventId, mockRequest);

      expect(result).toEqual(canceledEvent);
    });
  });

  describe("postponeEvent", () => {
    it("should postpone an event", async () => {
      const postponedEvent = { ...mockEvent, status: EventStatus.POSTPONED };
      jest
        .spyOn(service, "postponeEvent")
        .mockResolvedValue(postponedEvent as Event);

      const result = await controller.postponeEvent(mockEventId, mockRequest);

      expect(result).toEqual(postponedEvent);
    });
  });

  describe("rescheduleEvent", () => {
    it("should reschedule an event", async () => {
      const rescheduleEventDto: RescheduleEventDto = {
        date: "2025-04-29",
        startTime: "19:30",
        endTime: "22:30",
      };

      const rescheduledEvent = {
        ...mockEvent,
        status: EventStatus.RESCHEDULED,
        startDateTime: new Date(
          `${rescheduleEventDto.date}T${rescheduleEventDto.startTime}:00`
        ),
        endDateTime: new Date(
          `${rescheduleEventDto.date}T${rescheduleEventDto.endTime}:00`
        ),
      };

      jest
        .spyOn(service, "rescheduleEvent")
        .mockResolvedValue(rescheduledEvent as Event);

      const result = await controller.rescheduleEvent(
        mockEventId,
        rescheduleEventDto,
        mockRequest
      );

      expect(result).toEqual(rescheduledEvent);
    });
  });
});
