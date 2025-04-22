import {
  PipeTransform,
  Injectable,
  ArgumentMetadata,
  BadRequestException,
} from "@nestjs/common";
import { validate } from "class-validator";
import { plainToInstance } from "class-transformer";
import { LoggerService } from "../services/logger.service";

@Injectable()
export class CustomValidationPipe implements PipeTransform<any> {
  private readonly logger = new LoggerService(CustomValidationPipe.name);

  constructor(
    private readonly options?: {
      skipMissingProperties?: boolean;
      whitelist?: boolean;
      forbidNonWhitelisted?: boolean;
    }
  ) {}

  async transform(value: any, { metatype }: ArgumentMetadata) {
    if (!metatype || !this.toValidate(metatype)) {
      return value;
    }

    const object = plainToInstance(metatype, value);
    const errors = await validate(object, {
      skipMissingProperties: this.options?.skipMissingProperties ?? false,
      whitelist: this.options?.whitelist ?? true,
      forbidNonWhitelisted: this.options?.forbidNonWhitelisted ?? true,
    });

    if (errors.length > 0) {
      const messages = errors.map((error) => {
        const constraints = Object.values(error.constraints || {});
        return `${error.property}: ${constraints.join(", ")}`;
      });

      this.logger.error(`Validation failed: ${messages.join("; ")}`);
      throw new BadRequestException(messages);
    }

    return object;
  }

  private toValidate(metatype: Function): boolean {
    const types: Function[] = [String, Boolean, Number, Array, Object];
    return !types.includes(metatype);
  }
}
