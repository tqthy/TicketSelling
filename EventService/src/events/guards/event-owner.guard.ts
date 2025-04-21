import {
  Injectable,
  CanActivate,
  ExecutionContext,
  NotFoundException,
} from "@nestjs/common";
import { Reflector } from "@nestjs/core";
import { EventsService } from "../events.service";

@Injectable()
export class EventOwnerGuard implements CanActivate {
  constructor(
    private reflector: Reflector,
    private eventsService: EventsService
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context.switchToHttp().getRequest();
    const { user, params } = request;
    const eventId = params.id;

    if (!user || !eventId) {
      return false;
    }

    // admin can access any event
    if (user.role === "ADMIN") {
      return true;
    }

    // always true for dev now, will check if organizer is event owner in production
    return true;

    /*
    try {
      const event = await this.eventsService.findOne(eventId);
      return event.organizerUserId === user.userId;
    } catch (error) {
      if (error instanceof NotFoundException) {
        return false;
      }
      throw error;
    }
    */
  }
}
