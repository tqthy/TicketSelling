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
import { EventsService } from "./events.service";
import { CreateEventDto } from "./dto/create-event.dto";
import { UpdateEventDto } from "./dto/update-event.dto";
import { RescheduleEventDto } from "./dto/reschedule-event.dto";
import { RolesGuard } from "../auth/guards/roles.guard";
import { Roles } from "../auth/decorators/roles.decorator";

@Controller("events")
export class EventsController {
  constructor(private readonly eventsService: EventsService) {}

  @Post()
  @UseGuards(RolesGuard)
  @Roles("ORGANIZER", "ADMIN")
  create(@Body() createEventDto: CreateEventDto, @Req() req: RequestWithUser) {
    return this.eventsService.create(createEventDto, req.user.userId);
  }

  @Get()
  findAll() {
    return this.eventsService.findAll();
  }

  @Get("published")
  findAllPublished() {
    return this.eventsService.findAllPublished();
  }

  @Get(":id")
  findOne(@Param("id") id: string) {
    return this.eventsService.findOne(id);
  }

  @Patch(":id")
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  update(
    @Param("id") id: string,
    @Body() updateEventDto: UpdateEventDto,
    @Req() req: RequestWithUser
  ) {
    return this.eventsService.update(id, updateEventDto);
  }

  @Delete(":id")
  @HttpCode(HttpStatus.NO_CONTENT)
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  remove(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.remove(id);
  }

  @Patch(":id/approve")
  @UseGuards(RolesGuard)
  @Roles("ADMIN")
  approveEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.approveEvent(id);
  }

  @Patch(":id/submit-for-approval")
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ORGANIZER", "ADMIN")
  submitForApproval(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.submitForApproval(id);
  }

  @Patch(":id/cancel")
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  cancelEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.cancelEvent(id);
  }

  @Patch(":id/postpone")
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  postponeEvent(@Param("id") id: string, @Req() req: RequestWithUser) {
    return this.eventsService.postponeEvent(id);
  }

  @Patch(":id/reschedule")
  @UseGuards(RolesGuard, EventOwnerGuard)
  @Roles("ADMIN", "ORGANIZER")
  rescheduleEvent(
    @Param("id") id: string,
    @Body() rescheduleEventDto: RescheduleEventDto,
    @Req() req: RequestWithUser
  ) {
    return this.eventsService.rescheduleEvent(id, rescheduleEventDto);
  }
}
