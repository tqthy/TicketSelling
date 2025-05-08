import { DocumentBuilder, SwaggerModule } from "@nestjs/swagger";
import { INestApplication } from "@nestjs/common";

export function setupSwagger(app: INestApplication) {
  const config = new DocumentBuilder()
    .setTitle("Event Service API")
    .setDescription("API for managing events in the ticket selling system")
    .setVersion("1.0")
    .build();

  const document = SwaggerModule.createDocument(app, config, {
    deepScanRoutes: true,
  });

  addCustomExamplesToSwagger(document);

  SwaggerModule.setup("api", app, document, {
    swaggerOptions: {
      persistAuthorization: true,
      tagsSorter: "alpha",
      operationsSorter: "alpha",
    },
  });
}

function addCustomExamplesToSwagger(document: any) {
  // Example for Create Event endpoint
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
                date: "2025-07-15",
                startTime: "18:00",
                endTime: "23:00",
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
                date: "2025-08-20",
                startTime: "19:00",
                endTime: "22:00",
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

  // Example for Update Event endpoint
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

  // Example for Reschedule Event endpoint
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
                date: "2025-08-15",
                startTime: "18:00",
                endTime: "23:00",
              },
            },
            "Reschedule to Next Day": {
              summary: "Reschedule to the next day",
              description: "Reschedule a postponed event to the next day",
              value: {
                date: "2025-07-16",
                startTime: "18:00",
                endTime: "23:00",
              },
            },
          },
        },
      },
    };
  }

  addExamplesForStatusChangeEndpoints(document);
}

function addExamplesForStatusChangeEndpoints(document: any) {
  // These endpoints don't have request bodies, but we can add descriptions
  const statusEndpoints = [
    "/events/{id}/submit-for-approval",
    "/events/{id}/approve",
    "/events/{id}/publish",
    "/events/{id}/cancel",
    "/events/{id}/postpone",
  ];

  statusEndpoints.forEach((endpoint) => {
    if (document.paths[endpoint] && document.paths[endpoint].patch) {
      // Add more detailed descriptions
      document.paths[endpoint].patch.description =
        getDetailedDescription(endpoint);

      // Add parameter examples
      if (document.paths[endpoint].patch.parameters) {
        document.paths[endpoint].patch.parameters.forEach((param: any) => {
          if (param.name === "id") {
            param.example = "123e4567-e89b-12d3-a456-426614174000";
            param.description = "The UUID of the event";
          }
        });
      }
    }
  });
}

function getDetailedDescription(endpoint: string): string {
  switch (endpoint) {
    case "/events/{id}/submit-for-approval":
      return "Submits a draft event for approval by an admin. The event status will change from DRAFT to PENDING_APPROVAL.";
    case "/events/{id}/approve":
      return "Approves an event that has been submitted for approval. The event status will change from PENDING_APPROVAL to APPROVED. Only admins can approve events.";
    case "/events/{id}/publish":
      return "Publishes an approved event, making it visible to the public. The event status will change from APPROVED to PUBLISHED. Only admins can publish events.";
    case "/events/{id}/cancel":
      return "Cancels an event. The event status will change to CANCELLED. Both organizers and admins can cancel events.";
    case "/events/{id}/postpone":
      return "Postpones a published event. The event status will change from PUBLISHED to POSTPONED. Both organizers and admins can postpone events.";

    default:
      return "Changes the status of an event.";
  }
}
