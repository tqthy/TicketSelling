import { OmitType, PartialType } from "@nestjs/swagger";
import { CreateEventDto } from "./create-event.dto";
import {
  IsEnum,
  IsOptional,
  IsString,
  IsArray,
  IsNotEmpty,
  IsUUID,
} from "class-validator";

class CreateEventDtoWithoutDateAndPricing extends OmitType(CreateEventDto, [
  "startDateTime",
  "endDateTime",
  "sectionPricing",
] as const) {}

export class UpdateEventDto extends PartialType(
  CreateEventDtoWithoutDateAndPricing
) {
  @IsOptional()
  @IsString({ message: "Event name must be a string" })
  name?: string;

  @IsOptional()
  @IsString({ message: "Event description must be a string" })
  description?: string;

  @IsOptional()
  @IsEnum(["MATCH", "CONCERT", "OTHERS"], {
    message: "Category must be one of: MATCH, CONCERT, OTHERS",
  })
  category?: "MATCH" | "CONCERT" | "OTHERS";

  @IsOptional()
  @IsUUID("all", { message: "Venue ID must be a valid UUID" })
  venueId?: string;

  @IsOptional()
  @IsString({ message: "Venue name must be a string" })
  venueName?: string;

  @IsOptional()
  @IsString({ message: "Venue address must be a string" })
  venueAddress?: string;

  @IsOptional()
  @IsString({ message: "Poster URL must be a string" })
  poster?: string;

  @IsOptional()
  @IsArray({ message: "Images must be an array" })
  @IsString({ each: true, message: "Each image URL must be a string" })
  images?: string[];

  @IsOptional()
  @IsString({ message: "Details must be a string" })
  details?: string;
}
