import { Test, TestingModule } from "@nestjs/testing";
import { INestApplication, ValidationPipe } from "@nestjs/common";
import * as request from "supertest";
import { AppModule } from "../src/app.module";
import { JwtAuthGuard } from "../src/auth/guards/jwt-auth.guard";
import { EventOwnerGuard } from "../src/events/guards/event-owner.guard";
import { EventStatus } from "../src/events/entities/event.entity";
import { RolesGuard } from "../src/auth/guards/roles.guard";

// Mock guards
const mockJwtAuthGuard = {
  canActivate: jest.fn((context) => {
    // Mock the user object in the request
    const request = context.switchToHttp().getRequest();
    request.user = {
      userId: "organizer123",
      role: "ORGANIZER",
    };
    return true;
  }),
};
const mockRolesGuard = { canActivate: jest.fn(() => true) };
const mockEventOwnerGuard = { canActivate: jest.fn(() => true) };

describe("EventsController (e2e)", () => {
  let app: INestApplication;

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    })
      .overrideGuard(JwtAuthGuard)
      .useValue(mockJwtAuthGuard)
      .overrideGuard(RolesGuard)
      .useValue(mockRolesGuard)
      .overrideGuard(EventOwnerGuard)
      .useValue(mockEventOwnerGuard)
      .compile();

    app = moduleFixture.createNestApplication();
    app.useGlobalPipes(
      new ValidationPipe({
        whitelist: true,
        transform: true,
        forbidNonWhitelisted: true,
      })
    );
    await app.init();
  });

  afterAll(async () => {
    await app.close();
  });

  it("/events (GET)", () => {
    return request(app.getHttpServer())
      .get("/events")
      .expect(200)
      .expect((res) => {
        expect(Array.isArray(res.body)).toBe(true);
      });
  });

  it("/events/published (GET)", () => {
    return request(app.getHttpServer())
      .get("/events/published")
      .expect(200)
      .expect((res) => {
        expect(Array.isArray(res.body)).toBe(true);
      });
  });

  it("/events (POST) - validation error", () => {
    return request(app.getHttpServer())
      .post("/events")
      .send({
        // Missing required fields
        name: "Test Event",
      })
      .expect(400);
  });

  it("/events/:id (GET) - not found", () => {
    // Use a valid UUID format that doesn't exist in the database
    const nonExistentId = "00000000-0000-0000-0000-000000000000";
    return request(app.getHttpServer())
      .get(`/events/${nonExistentId}`)
      .expect(404);
  });

  // Note: The following tests would require proper authentication and mocking
  // of the database. For now, we'll focus on the endpoints that don't require
  // authentication and are more predictable in their behavior.
});
