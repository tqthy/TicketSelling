import {
  IsArray,
  IsEnum,
  IsNotEmpty,
  IsOptional,
  IsString,
  IsUUID,
  ValidateNested,
} from "class-validator";
import {
  IsFutureDate,
  IsDateTimeAfter,
} from "../../common/validators/date-validators";
import { Type } from "class-transformer";
import { EventSectionPricingDto } from "./event-section-pricing.dto";

export class CreateEventDto {
  @IsNotEmpty({ message: "Event name is required" })
  @IsString({ message: "Event name must be a string" })
  name: string;

  @IsNotEmpty({ message: "Event description is required" })
  @IsString({ message: "Event description must be a string" })
  description: string;

  @IsNotEmpty({ message: "Event category is required" })
  @IsEnum(["MATCH", "CONCERT", "OTHERS"], {
    message: "Category must be one of: MATCH, CONCERT, OTHERS",
  })
  category: "MATCH" | "CONCERT" | "OTHERS";

  @IsNotEmpty({ message: "Start date and time is required" })
  @IsString({ message: "Start date and time must be a string" })
  @IsFutureDate({ message: "Event start date and time must be in the future" })
  startDateTime: string;

  @IsNotEmpty({ message: "End date and time is required" })
  @IsString({ message: "End date and time must be a string" })
  @IsDateTimeAfter("startDateTime", {
    message: "End date and time must be after start date and time",
  })
  endDateTime: string;

  @IsNotEmpty({ message: "Venue ID is required" })
  @IsUUID("all", { message: "Venue ID must be a valid UUID" })
  venueId: string;

  @IsNotEmpty({ message: "Venue name is required" })
  @IsString({ message: "Venue name must be a string" })
  venueName: string;

  @IsNotEmpty({ message: "Venue address is required" })
  @IsString({ message: "Venue address must be a string" })
  venueAddress: string;

  @IsNotEmpty({ message: "Poster URL is required" })
  @IsString({ message: "Poster URL must be a string" })
  poster: string;

  @IsOptional()
  @IsArray({ message: "Images must be an array" })
  @IsString({ each: true, message: "Each image URL must be a string" })
  images?: string[];

  @IsOptional()
  @IsString({ message: "Details must be a string" })
  details?: string;

  @IsArray({ message: "Section pricing must be an array" })
  @ValidateNested({ each: true })
  @Type(() => EventSectionPricingDto)
  sectionPricing: EventSectionPricingDto[];
}
