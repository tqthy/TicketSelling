import { Injectable, NestMiddleware } from "@nestjs/common";
import { Request, Response, NextFunction } from "express";
import { LoggerService } from "../services/logger.service";

@Injectable()
export class TestUserInjectionMiddleware implements NestMiddleware {
  private readonly logger = new LoggerService(TestUserInjectionMiddleware.name);

  use(req: Request, res: Response, next: NextFunction) {
    req.user = {
      userId: "123",
      role: "ADMIN",
    };

    this.logger.log(
      `Injected test admin user into request: ${JSON.stringify(req.user)}`
    );
    next();
  }
}
