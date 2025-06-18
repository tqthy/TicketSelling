# Ticket Selling Platform

This is a comprehensive, microservices-based platform for selling event tickets. It is built with a modern technology stack including .NET, Node.js, Docker, and various AWS services. The platform is designed to be scalable, maintainable, and robust, providing a complete solution for event management, ticket booking, payment processing, and user management.

## Features

* **User Management:** Handles user registration, authentication (including Google OAuth), and profile management with role-based access control (Admin, Organizer, User).
* **Event Management:** Allows organizers to create, manage, and publish events. It includes a status workflow for events (Draft, Pending Approval, Published, etc.).
* **Venue Management:** Provides functionalities to create and manage venues, including sections and seats.
* **Booking System:** Enables users to book tickets for events, with real-time seat availability checks.
* **Payment Processing:** Integrates with payment gateways like VNPay for secure transaction processing.
* **Notifications:** Sends email notifications for significant events like successful payments or user registration.
* **API Gateway:** A single entry point for all client requests, routing them to the appropriate microservices.
* **CI/CD and Deployment:** Automated build, testing, and deployment pipeline using GitHub Actions to deploy services as Docker containers on AWS.

## Architecture

The platform is built on a microservices architecture, with each service responsible for a specific business domain. This design promotes loose coupling, independent scalability, and easier maintenance.
<img width="984" alt="image" src="https://github.com/user-attachments/assets/bd22fa45-ddde-4c9b-8a43-55a16667665b" />

* **UserService (.NET):** Manages user authentication, registration, and user data.
* **EventService (Node.js/NestJS):** Handles event creation, updates, and status changes.
* **VenueService (.NET):** Manages venue information, including sections and seat layouts.
* **BookingService (.NET):** Manages the ticket booking process.
* **PaymentService (.NET):** Processes payments through various gateways.
* **NotificationService (.NET):** Sends notifications to users.
* **ApiGateway (.NET/Ocelot):** Acts as a reverse proxy, routing requests to the appropriate services.

These services communicate with each other through a combination of synchronous REST APIs and asynchronous messaging using RabbitMQ.

## Getting Started

### Prerequisites

* .NET SDK (version specified in project files)
* Node.js and npm
* Docker and Docker Compose
* AWS CLI, configured with your credentials

### Local Development

1.  **Clone the repository.**
2.  **Set up environment variables:** Create a `.env` file in the `scripts` directory with the necessary configurations for databases, JWT secrets, and other services. You can use the provided `.env.example` as a template.
3.  **Run the services using Docker Compose:**
    ```bash
    cd scripts
    docker-compose -f docker-compose.dev.yml up --build
    ```
    This will build and start all the microservices, along with dependent services like RabbitMQ.
4.  **Access the services:**
    * **API Gateway:** `http://localhost:8080`
    * **EventService API Documentation (Swagger):** `http://localhost:8081/api`

## Technologies Used

* **Backend:** .NET, Node.js (NestJS)
* **Containerization:** Docker
* **Database:** PostgreSQL
* **Messaging:** RabbitMQ
* **API Gateway:** Ocelot
* **Infrastructure as Code:** Terraform
* **CI/CD:** GitHub Actions
* **Cloud Provider:** AWS (EC2, RDS, S3)

## Deployment

The project includes a CI/CD pipeline defined in `.github/workflows/aws.yml`. When code is pushed to the `main` branch, the following steps are executed:

1.  **Build and Push Docker Images:** Each service's Docker image is built and pushed to a container registry.
2.  **Deploy to EC2:** The new images are deployed to a Docker Swarm cluster running on AWS EC2 instances.

The infrastructure is managed using Terraform, with the configuration located in the `Terraform` directory. For more details, see the [Terraform README](./Terraform/README.md).
