import { DocumentBuilder, SwaggerModule } from "@nestjs/swagger";
import { INestApplication } from "@nestjs/common";

export function setupSwagger(app: INestApplication) {
  const config = new DocumentBuilder()
    .setTitle("Event Service Documentation")
    .setDescription("API for managing events in the ticket selling system")
    .setVersion("1.0")
    .build();

  const document = SwaggerModule.createDocument(app, config, {
    deepScanRoutes: true,
  });

  document.paths = {};
  defineEventRoutes(document);
  addCustomExamplesToSwagger(document);

  SwaggerModule.setup("api", app, document, {
    swaggerOptions: {
      persistAuthorization: true,
      tagsSorter: "alpha",
      operationsSorter: "alpha",
    },
  });
}

function defineEventRoutes(document: any) {
  // POST /events - Create a new event
  document.paths["/events"] = {
    post: {
      tags: ["events"],
      summary: "Create a new event",
      operationId: "create",
      security: [{ bearer: [] }],
      requestBody: {
        description: "Event to create",
        required: true,
        content: {
          "application/json": {
            schema: {
              $ref: "#/components/schemas/CreateEventDto",
            },
          },
        },
      },
      responses: {
        "201": { description: "The event has been successfully created." },
        "400": { description: "Bad Request - Invalid input data." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
      },
    },
    get: {
      tags: ["events"],
      summary: "Get all events",
      operationId: "findAll",
      responses: {
        "200": { description: "Return all events." },
      },
    },
  };

  // GET /events/published - Get all published events
  document.paths["/events/published"] = {
    get: {
      tags: ["events"],
      summary: "Get all published events",
      operationId: "findAllPublished",
      responses: {
        "200": { description: "Return all published events." },
      },
    },
  };

  // GET /events/{id} - Get a specific event by ID
  document.paths["/events/{id}"] = {
    get: {
      tags: ["events"],
      summary: "Get a specific event by ID",
      operationId: "findOne",
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "200": { description: "Return the event." },
        "404": { description: "Event not found." },
      },
    },
    patch: {
      tags: ["events"],
      summary: "Update an event",
      operationId: "update",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      requestBody: {
        description: "Event data to update",
        required: true,
        content: {
          "application/json": {
            schema: {
              $ref: "#/components/schemas/UpdateEventDto",
            },
          },
        },
      },
      responses: {
        "200": { description: "The event has been successfully updated." },
        "400": { description: "Bad Request - Invalid input data." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found." },
      },
    },
    delete: {
      tags: ["events"],
      summary: "Delete an event",
      operationId: "remove",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "204": { description: "The event has been successfully deleted." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found." },
      },
    },
  };

  // PATCH /events/{id}/approve - Approve and publish an event
  document.paths["/events/{id}/approve"] = {
    patch: {
      tags: ["events"],
      summary: "Approve and publish an event",
      description:
        "Approves an event that has been submitted for approval. The event status will change from PENDING_APPROVAL to APPROVED. Only admins can approve events.",
      operationId: "approveEvent",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "200": {
          description:
            "The event has been successfully approved and published.",
        },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found or not in the correct status." },
      },
    },
  };

  // PATCH /events/{id}/submit-for-approval - Submit an event for approval
  document.paths["/events/{id}/submit-for-approval"] = {
    patch: {
      tags: ["events"],
      summary: "Submit an event for approval",
      description:
        "Submits a draft event for approval by an admin. The event status will change from DRAFT to PENDING_APPROVAL.",
      operationId: "submitForApproval",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "200": {
          description:
            "The event has been successfully submitted for approval.",
        },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found or not in the correct status." },
      },
    },
  };

  // PATCH /events/{id}/cancel - Cancel an event
  document.paths["/events/{id}/cancel"] = {
    patch: {
      tags: ["events"],
      summary: "Cancel an event",
      description:
        "Cancels an event. The event status will change to CANCELLED. Both organizers and admins can cancel events.",
      operationId: "cancelEvent",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "200": { description: "The event has been successfully canceled." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found or not in the correct status." },
      },
    },
  };

  // PATCH /events/{id}/postpone - Postpone an event
  document.paths["/events/{id}/postpone"] = {
    patch: {
      tags: ["events"],
      summary: "Postpone an event",
      description:
        "Changes a published event's status to Postponed, allowing it to be rescheduled later",
      operationId: "postponeEvent",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      responses: {
        "200": { description: "The event has been successfully postponed." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found or not in the correct status." },
      },
    },
  };

  // PATCH /events/{id}/reschedule - Reschedule a postponed event
  document.paths["/events/{id}/reschedule"] = {
    patch: {
      tags: ["events"],
      summary: "Reschedule a postponed event",
      description:
        "Sets new date and time for a postponed event and changes its status to Rescheduled",
      operationId: "rescheduleEvent",
      security: [{ bearer: [] }],
      parameters: [
        {
          name: "id",
          in: "path",
          description: "Event ID",
          required: true,
          schema: {
            type: "string",
            example: "123e4567-e89b-12d3-a456-426614174000",
          },
        },
      ],
      requestBody: {
        description: "New date and time for the event",
        required: true,
        content: {
          "application/json": {
            schema: {
              $ref: "#/components/schemas/RescheduleEventDto",
            },
          },
        },
      },
      responses: {
        "200": { description: "The event has been successfully rescheduled." },
        "400": { description: "Bad Request - Invalid input data." },
        "401": { description: "Unauthorized." },
        "403": { description: "Forbidden." },
        "404": { description: "Event not found or not in the correct status." },
      },
    },
  };
}

function addCustomExamplesToSwagger(document: any) {
  if (document.paths["/events"] && document.paths["/events"].post) {
    document.paths["/events"].post.requestBody = {
      ...document.paths["/events"].post.requestBody,
      content: {
        "application/json": {
          schema:
            document.paths["/events"].post.requestBody.content[
              "application/json"
            ].schema,
          examples: {
            "Complete Event": {
              summary: "Complete event with all required fields",
              description:
                "A complete event with all required fields and section pricing",
              value: {
                name: "Real Madrid vs Barcelona",
                description: "A match between Real Madrid and Barcelona",
                category: "MATCH",
                startDateTime: "2025-07-15T18:00:00",
                endDateTime: "2025-07-15T23:00:00",
                venueId: "123e4567-e89b-12d3-a456-426614174000",
                venueName: "Central Park Arena",
                venueAddress: "123 Park Avenue, New York, NY 10001",
                poster: "https://example.com/posters/summer-fest.jpg",
                images: [
                  "https://example.com/images/summer-fest-1.jpg",
                  "https://example.com/images/summer-fest-2.jpg",
                ],
                details: "<p>Bring your own chairs and blankets</p>",
                sectionPricing: [
                  {
                    sectionId: "123e4567-e89b-12d3-a456-426614174001",
                    price: 150.0,
                  },
                  {
                    sectionId: "123e4567-e89b-12d3-a456-426614174002",
                    price: 100.0,
                  },
                  {
                    sectionId: "123e4567-e89b-12d3-a456-426614174003",
                    price: 75.0,
                  },
                ],
              },
            },
            "Minimal Event": {
              summary: "Event with only required fields",
              description: "An event with only the required fields",
              value: {
                name: "Jazz Night",
                description: "An evening of jazz music",
                category: "CONCERT",
                startDateTime: "2025-08-20T19:00:00",
                endDateTime: "2025-08-20T22:00:00",
                venueId: "123e4567-e89b-12d3-a456-426614174000",
                venueName: "Jazz Club",
                venueAddress: "456 Music Street, New York, NY 10002",
                poster: "https://example.com/posters/jazz-night.jpg",
                sectionPricing: [
                  {
                    sectionId: "123e4567-e89b-12d3-a456-426614174001",
                    price: 50.0,
                  },
                ],
              },
            },
          },
        },
      },
    };
  }

  if (document.paths["/events/{id}"] && document.paths["/events/{id}"].patch) {
    document.paths["/events/{id}"].patch.requestBody = {
      ...document.paths["/events/{id}"].patch.requestBody,
      content: {
        "application/json": {
          schema:
            document.paths["/events/{id}"].patch.requestBody.content[
              "application/json"
            ].schema,
          examples: {
            "Update Event Details": {
              summary: "Update basic event details",
              description:
                "Update the name, description, category, and poster URL of an event",
              value: {
                name: "Summer Music Festival 2025",
                description:
                  "A weekend of live music performances featuring top artists",
                category: "CONCERT",
                poster: "https://example.com/posters/summer-fest-updated.jpg",
                images: [
                  "https://example.com/images/summer-fest-updated-1.jpg",
                  "https://example.com/images/summer-fest-updated-2.jpg",
                ],
                details:
                  "Bring your own chairs and blankets. Food vendors will be available.",
              },
            },
          },
        },
      },
    };
  }

  if (
    document.paths["/events/{id}/reschedule"] &&
    document.paths["/events/{id}/reschedule"].patch
  ) {
    document.paths["/events/{id}/reschedule"].patch.requestBody = {
      ...document.paths["/events/{id}/reschedule"].patch.requestBody,
      content: {
        "application/json": {
          schema:
            document.paths["/events/{id}/reschedule"].patch.requestBody.content[
              "application/json"
            ].schema,
          examples: {
            "Reschedule Event": {
              summary: "Reschedule a postponed event",
              description: "Set new date and time for a postponed event",
              value: {
                newStartDateTime: "2025-08-15T18:00:00",
                newEndDateTime: "2025-08-15T23:00:00",
              },
            },
            "Reschedule to Next Day": {
              summary: "Reschedule to the next day",
              description: "Reschedule a postponed event to the next day",
              value: {
                newStartDateTime: "2025-07-16T18:00:00",
                newEndDateTime: "2025-07-16T23:00:00",
              },
            },
          },
        },
      },
    };
  }
}
