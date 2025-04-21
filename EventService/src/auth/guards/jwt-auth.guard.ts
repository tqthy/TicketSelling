import { Injectable, ExecutionContext } from "@nestjs/common";
import { AuthGuard } from "@nestjs/passport";

@Injectable()
export class JwtAuthGuard extends AuthGuard("jwt") {
  canActivate(context: ExecutionContext) {
    // for dev, will validate jwt token in production
    const request = context.switchToHttp().getRequest();

    // mock user data for dev
    request.user = {
      userId: "123e4567-e89b-12d3-a456-426614174000",
      role: "ADMIN",
    };

    return true;
  }
}
