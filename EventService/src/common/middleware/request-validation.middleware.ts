import { Injectable, NestMiddleware } from "@nestjs/common";
import { Request, Response, NextFunction } from "express";
import { LoggerService } from "../services/logger.service";

@Injectable()
export class RequestValidationMiddleware implements NestMiddleware {
  private readonly logger = new LoggerService(RequestValidationMiddleware.name);

  use(req: Request, res: Response, next: NextFunction) {
    // log incoming requests
    this.logger.log(`${req.method} ${req.originalUrl}`);

    // validate request body if it exists
    if (req.body && Object.keys(req.body).length > 0) {
      this.logger.debug(`Request body: ${JSON.stringify(req.body)}`);
    }

    // validate request parameters if they exist
    if (req.params && Object.keys(req.params).length > 0) {
      this.logger.debug(`Request params: ${JSON.stringify(req.params)}`);
    }

    // validate request query if it exists
    if (req.query && Object.keys(req.query).length > 0) {
      this.logger.debug(`Request query: ${JSON.stringify(req.query)}`);
    }

    next();
  }
}
