import {
  Injectable,
  LoggerService as NestLoggerService,
  Scope,
} from "@nestjs/common";

@Injectable({ scope: Scope.TRANSIENT })
export class LoggerService implements NestLoggerService {
  private context?: string;

  constructor(context?: string) {
    this.context = context;
  }

  setContext(context: string) {
    this.context = context;
  }

  log(message: any, context?: string) {
    this.printMessage("LOG", message, context);
  }

  error(message: any, trace?: string, context?: string) {
    this.printMessage("ERROR", message, context);
    if (trace) {
      console.error(trace);
    }
  }

  warn(message: any, context?: string) {
    this.printMessage("WARN", message, context);
  }

  debug(message: any, context?: string) {
    this.printMessage("DEBUG", message, context);
  }

  verbose(message: any, context?: string) {
    this.printMessage("VERBOSE", message, context);
  }

  private printMessage(level: string, message: any, context?: string) {
    const timestamp = new Date().toISOString();
    const formattedContext = context || this.context || "Application";

    console.log(`[${timestamp}] [${level}] [${formattedContext}] ${message}`);
  }
}
