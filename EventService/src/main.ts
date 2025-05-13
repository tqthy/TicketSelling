import { NestFactory } from "@nestjs/core";
import { AppModule } from "./app.module";
import { ValidationPipe, BadRequestException } from "@nestjs/common";
import {
  HttpExceptionFilter,
  AllExceptionsFilter,
} from "./common/filters/http-exception.filter";
import { setupSwagger } from "../swagger-config";
import { MetricsInterceptor } from "./metrics/metrics.interceptor";

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  app.enableCors();

  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      transform: true,
      forbidNonWhitelisted: true,
      transformOptions: { enableImplicitConversion: true },
      exceptionFactory: (errors) => {
        const messages = errors.map((error) => {
          const constraints = Object.values(error.constraints || {});
          return `${error.property}: ${constraints.join(", ")}`;
        });
        return new BadRequestException(messages);
      },
    })
  );

  app.useGlobalFilters(new AllExceptionsFilter(), new HttpExceptionFilter());
  
  // Use MetricsInterceptor for all routes except /metrics
  const metricsInterceptor = app.get(MetricsInterceptor);
  app.useGlobalInterceptors(metricsInterceptor);
  
  setupSwagger(app);

  const port = process.env.PORT || 3000;
  await app.listen(port);
  console.log(`Application is running on: http://localhost:${port}`);
}
bootstrap();
