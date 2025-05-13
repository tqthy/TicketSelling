import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
  Logger,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { MetricsService } from './metrics.service';

@Injectable()
export class MetricsInterceptor implements NestInterceptor {
  private readonly logger = new Logger(MetricsInterceptor.name);

  constructor(private readonly metricsService: MetricsService) {}

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const req = context.switchToHttp().getRequest();
    if (req.url === '/metrics') {
      return next.handle();
    }

    const method = req.method;
    const url = req.originalUrl || req.url;
    
    const endTimer = this.metricsService.startTimer(method, url);
    
    return next.handle().pipe(
      tap({
        next: () => {
          const res = context.switchToHttp().getResponse();
          const status = res.statusCode;
          
          this.metricsService.recordRequest(method, url, status);
          
          endTimer();
        },
      }),
      catchError(error => {
        const status = error.status || 500;
        
        this.metricsService.recordRequest(method, url, status);
        
        endTimer();
        
        this.logger.error(
          `Request ${method} ${url} failed with status ${status}`,
          error.stack
        );
        
        throw error;
      }),
    );
  }
}