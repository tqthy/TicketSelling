import {
  Controller,
  Get,
  Post,
  Body,
  Patch,
  Param,
  Delete,
  UseGuards,
  Req,
  HttpStatus,
  HttpCode,
} from "@nestjs/common";
import { RequestWithUser } from "../auth/interfaces/request-with-user.interface";
import { EventOwnerGuard } from "./guards/event-owner.guard";
import { RequireEventOwner } from "./decorators/require-event-owner.decorator";
import { EventsService } from "./events.service";
import { CreateEventDto } from "./dto/create-event.dto";
import { UpdateEventDto } from "./dto/update-event.dto";
import { RescheduleEventDto } from "./dto/reschedule-event.dto";
import { JwtAuthGuard } from "../auth/guards/jwt-auth.guard";
import {
  ApiTags,
  ApiOperation,
  ApiResponse,
  ApiBearerAuth,
  ApiParam,
} from "@nestjs/swagger";
import { RolesGuard } from "../auth/guards/roles.guard";
import { Roles } from "../auth/decorators/roles.decorator";

@ApiTags("events")
@Controller("events")
export class EventsController {
  constructor(private readonly eventsService: EventsService) {}

  @Post()
  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles("ORGANIZER", "ADMIN")
  @ApiBearerAuth()
  @ApiOperation({ summary: "Create a new event" })
  @ApiResponse({
    status: 201,
    description: "The event has been successfully created.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 400,
    description: "Bad Request - Invalid input data.",
  })
  create(@Body() createEventDto: CreateEventDto, @Req() req: RequestWithUser) {
    return this.eventsService.create(createEventDto, req.user.userId);
  }

  @Get()
  @ApiOperation({ summary: "Get all events" })
  @ApiResponse({ status: 200, description: "Return all events." })
  findAll() {
    return this.eventsService.findAll();
  }

  @Get("published")
  @ApiOperation({ summary: "Get all published events" })
  @ApiResponse({ status: 200, description: "Return all published events." })
  findAllPublished() {
    return this.eventsService.findAllPublished();
  }

  @Get(":id")
  @ApiOperation({ summary: "Get a specific event by ID" })
  @ApiResponse({ status: 200, description: "Return the event." })
  @ApiResponse({ status: 404, description: "Event not found." })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  findOne(@Param("id") id: string) {
    return this.eventsService.findOne(id);
  }

  @Patch(":id")
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({ summary: "Update an event" })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully updated.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({ status: 404, description: "Event not found." })
  @ApiResponse({
    status: 400,
    description: "Bad Request - Invalid input data.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  update(
    @Param("id") id: string,
    @Body() updateEventDto: UpdateEventDto,
    @Req() req: RequestWithUser
  ) {
    return this.eventsService.update(
      id,
      updateEventDto,
      req.user.userId,
      req.user.role
    );
  }

  @Delete(":id")
  @HttpCode(HttpStatus.NO_CONTENT)
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({ summary: "Delete an event" })
  @ApiResponse({
    status: 204,
    description: "The event has been successfully deleted.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({ status: 404, description: "Event not found." })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  remove(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.remove(id, req.user.userId, req.user.role);
  }

  @Patch(":id/approve")
  @UseGuards(JwtAuthGuard, RolesGuard)
  @Roles("ADMIN")
  @ApiBearerAuth()
  @ApiOperation({ summary: "Approve and publish an event" })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully approved and published.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 404,
    description: "Event not found or not in the correct status.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  approveEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.approveEvent(id, req.user.role);
  }

  @Patch(":id/submit-for-approval")
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ORGANIZER", "ADMIN")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({ summary: "Submit an event for approval" })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully submitted for approval.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 404,
    description: "Event not found or not in the correct status.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  submitForApproval(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.submitForApproval(
      id,
      req.user.userId,
      req.user.role
    );
  }

  @Patch(":id/cancel")
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({ summary: "Cancel an event" })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully canceled.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 404,
    description: "Event not found or not in the correct status.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  cancelEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.cancelEvent(id, req.user.userId, req.user.role);
  }

  @Patch(":id/postpone")
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({
    summary: "Postpone an event",
    description:
      "Changes a published event's status to Postponed, allowing it to be rescheduled later",
  })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully postponed.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 404,
    description: "Event not found or not in the correct status.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  postponeEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.postponeEvent(id, req.user.userId, req.user.role);
  }

  @Patch(":id/reschedule")
  @UseGuards(JwtAuthGuard, RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  @RequireEventOwner()
  @ApiBearerAuth()
  @ApiOperation({
    summary: "Reschedule a postponed event",
    description:
      "Sets new date and time for a postponed event and changes its status to Rescheduled",
  })
  @ApiResponse({
    status: 200,
    description: "The event has been successfully rescheduled.",
  })
  @ApiResponse({ status: 401, description: "Unauthorized." })
  @ApiResponse({ status: 403, description: "Forbidden." })
  @ApiResponse({
    status: 404,
    description: "Event not found or not in the correct status.",
  })
  @ApiResponse({
    status: 400,
    description: "Bad Request - Invalid input data.",
  })
  @ApiParam({
    name: "id",
    description: "Event ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  rescheduleEvent(
    @Param("id") id: string,
    @Body() rescheduleEventDto: RescheduleEventDto,
    @Req() req: RequestWithUser
  ) {
    return this.eventsService.rescheduleEvent(
      id,
      rescheduleEventDto,
      req.user.userId,
      req.user.role
    );
  }
}
