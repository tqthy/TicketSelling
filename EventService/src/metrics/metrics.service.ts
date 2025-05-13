import { Injectable, Logger } from '@nestjs/common';
import { InjectMetric } from '@willsoto/nestjs-prometheus';
import { Counter, Histogram } from 'prom-client';

@Injectable()
export class MetricsService {
  private readonly logger = new Logger(MetricsService.name);

  constructor(
    @InjectMetric('http_request_total')
    private readonly requestCounter: Counter<string>,
    
    @InjectMetric('http_request_duration_seconds')
    private readonly requestDuration: Histogram<string>,
  ) {
    this.logger.log('MetricsService initialized');
  }

  recordRequest(method: string, route: string, status: number) {
    try {
      this.requestCounter.inc({ 
        method, 
        route: this.normalizeRoute(route), 
        status: status.toString() 
      });
    } catch (error) {
      this.logger.error(`Failed to record request metric: ${error.message}`);
    }
  }

  startTimer(method: string, route: string): () => void {
    try {
      return this.requestDuration.startTimer({ 
        method, 
        route: this.normalizeRoute(route) 
      });
    } catch (error) {
      this.logger.error(`Failed to start timer: ${error.message}`);
      return () => {};
    }
  }

  private normalizeRoute(url: string): string {
    if (!url) return 'unknown';
    
    const routePath = url.split('?')[0];
    
    const normalizedPath = routePath
      .replace(/\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/g, '/:id')
      // Replace all numbers with :id
      .replace(/\/\d+/g, '/:id');
      
    return normalizedPath;
  }
}

