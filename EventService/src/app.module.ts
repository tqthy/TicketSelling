import { Module, MiddlewareConsumer, RequestMethod } from "@nestjs/common";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { TypeOrmModule } from "@nestjs/typeorm";
import { EventsModule } from "./events/events.module";
import { Event } from "./events/entities/event.entity";
import { EventSectionPricing } from "./events/entities/event-section-pricing.entity";
import { AuthModule } from "./auth/auth.module";
import { HealthModule } from "./health/health.module";
import { MetricsModule } from "./metrics/metrics.module";
import { RequestValidationMiddleware } from "./common/middleware/request-validation.middleware";
import { TestUserInjectionMiddleware } from "./common/middleware/test-user-injection.middleware";
import { MetricsInterceptor } from "./metrics/metrics.interceptor";

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
    }),
    TypeOrmModule.forRootAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        type: "postgres",
        host: configService.get("DB_HOST", "localhost"),
        port: configService.get<number>("DB_PORT", 5432),
        username: configService.get("DB_USERNAME", "postgres"),
        password: configService.get("DB_PASSWORD", "postgres"),
        database: configService.get("DB_DATABASE", "event_service"),
        entities: [Event, EventSectionPricing],
        synchronize: configService.get<boolean>("DB_SYNCHRONIZE", true),
        ssl: {
          rejectUnauthorized: false,
        },
      }),
    }),
    MetricsModule,
    EventsModule,
    AuthModule,
    HealthModule,
  ],
  providers: [MetricsInterceptor],
})
export class AppModule {
  configure(consumer: MiddlewareConsumer) {
    consumer
      .apply(TestUserInjectionMiddleware, RequestValidationMiddleware)
      .exclude('metrics')
      .forRoutes({ path: "*", method: RequestMethod.ALL });
  }
}
