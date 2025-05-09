import { Injectable, CanActivate, ExecutionContext } from "@nestjs/common";

@Injectable()
export class EventOwnerGuard implements CanActivate {
  // constructor(private eventsService: EventsService) {}

  constructor() {}

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

    // always true for dev now
    return true;

    /*
    // For production, uncomment this code and inject EventsService in the constructor
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
