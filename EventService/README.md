# Event Service

This is a NestJS-based microservice for managing events in the Ticket Selling System.

## Description

The Event Service provides CRUD operations for events and handles event status management, including approval, publishing, cancellation, postponement, and rescheduling.

## Features

- Create, read, update, and delete events
- Handle event status workflow (draft, submit for approval, published, postponed, rescheduled, canceled)
- Role-based access control (Admin, Organizer) with resource ownership validation
- Comprehensive validation using class-validator and custom validators
- Consistent error handling with global exception filters
- Detailed logging for better debugging and monitoring
- API documentation with Swagger (http://localhost:3000/api)
- Unit and E2E testing

## Event Status Flow

Events follow a specific status flow:

1. **Draft**: Initial status when an event is created
2. **Submit for Approval**: Organizer submits the event for admin approval
3. **Published**: Admin approves the event
4. **Postponed**: Event is postponed (can be rescheduled later)
5. **Rescheduled**: Postponed event is rescheduled with new date/time
6. **Canceled**: Event is canceled

Status transitions are strictly controlled and validated:

- Draft → Submit for Approval (by Organizer)
- Submit for Approval → Published (by Admin)
- Published → Postponed (by Organizer or Admin)
- Published → Canceled (by Organizer or Admin)
- Postponed → Rescheduled (by Organizer or Admin)

## Installation

```bash
$ npm install
```

## Running the app

```bash
# development
$ npm run start

# watch mode
$ npm run start:dev

# production mode
$ npm run start:prod
```

## Test

```bash
# unit tests
$ npm run test

# e2e tests
$ npm run test:e2e

# test coverage
$ npm run test:cov
```

## API Documentation

Once the application is running, you can access the Swagger documentation at:

```
http://localhost:3000/api
```

## Architecture

The service follows a layered architecture:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Implement business logic
- **DTOs**: Define data transfer objects for validation
- **Entities**: Define database models
- **Guards**: Handle authorization
- **Pipes**: Handle validation
- **Filters**: Handle exceptions
- **Middleware**: Process requests before they reach the route handler

## Docker Support

The service includes a high-performance Docker configuration:

- Multi-stage build process for smaller production images
- Node.js 20 Alpine base image for better performance and security
- Non-root user execution for enhanced security
- Health check endpoint for container orchestration
- Optimized Node.js runtime settings
- Proper caching of npm dependencies

To build and run with Docker:

```bash
# Build the Docker image
$ docker build -t event-service .

# Run the container
$ docker run -p 3000:3000 event-service
```

## Environment Variables

Create a `.env` file in the root directory with the following variables:

```
# Database Configuration
DB_HOST=localhost
DB_PORT=5432
DB_USERNAME=postgres
DB_PASSWORD=
DB_DATABASE=event_service
DB_SYNCHRONIZE=true

# JWT Configuration
JWT_SECRET=your-secret-key

# Application Configuration
PORT=3000
```

## License

This project is [MIT licensed](LICENSE).
