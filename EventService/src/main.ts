import { NestFactory } from "@nestjs/core";
import { AppModule } from "./app.module";
import { ValidationPipe, BadRequestException } from "@nestjs/common";
import {
  HttpExceptionFilter,
  AllExceptionsFilter,
} from "./common/filters/http-exception.filter";
import { setupSwagger } from "../swagger-config";

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Enable CORS
  app.enableCors();

  // Enable validation
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

  // Global exception filters
  app.useGlobalFilters(new AllExceptionsFilter(), new HttpExceptionFilter());

  // Set up Swagger documentation
  setupSwagger(app);

  // Start the application
  const port = process.env.PORT || 3000;
  await app.listen(port);
  console.log(`Application is running on: http://localhost:${port}`);
}
bootstrap();
