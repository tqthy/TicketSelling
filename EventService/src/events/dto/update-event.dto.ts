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
// Status is managed internally
import { ApiProperty } from "@nestjs/swagger";

// Create a type that omits the fields we don't want to update
class CreateEventDtoWithoutDateAndPricing extends OmitType(CreateEventDto, [
  "date",
  "startTime",
  "endTime",
  "startDateTime",
  "endDateTime",
  "sectionPricing",
] as const) {}

export class UpdateEventDto extends PartialType(
  CreateEventDtoWithoutDateAndPricing
) {
  @ApiProperty({
    description: "Event name (optional)",
    example: "Bien Hoa FC vs. Dong Nai FC",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Event name must be a string" })
  name?: string;

  @ApiProperty({
    description: "Event description (optional)",
    example: "A crucial match in the V.League",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Event description must be a string" })
  description?: string;

  @ApiProperty({
    description: "Event category (optional)",
    enum: ["MATCH", "CONCERT", "OTHERS"],
    example: "MATCH",
    required: false,
  })
  @IsOptional()
  @IsEnum(["MATCH", "CONCERT", "OTHERS"], {
    message: "Category must be one of: MATCH, CONCERT, OTHERS",
  })
  category?: "MATCH" | "CONCERT" | "OTHERS";

  @ApiProperty({
    description: "Venue ID (optional)",
    example: "123e4567-e89b-12d3-a456-426614174000",
    required: false,
  })
  @IsOptional()
  @IsUUID("all", { message: "Venue ID must be a valid UUID" })
  venueId?: string;

  @ApiProperty({
    description: "Venue name (optional)",
    example: "Can Tho Stadium",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Venue name must be a string" })
  venueName?: string;

  @ApiProperty({
    description: "Venue address (optional)",
    example: "Cần Thơ, Phường Cái Khế, Quận Ninh Kiều, Thành Phố Cần Thơ",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Venue address must be a string" })
  venueAddress?: string;

  @ApiProperty({
    description: "Event poster URL (optional)",
    example: "https://example.com/poster.jpg",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Poster URL must be a string" })
  poster?: string;

  @ApiProperty({
    description: "Event images URLs (optional)",
    example: [
      "https://example.com/image1.jpg",
      "https://example.com/image2.jpg",
    ],
    required: false,
    isArray: true,
  })
  @IsOptional()
  @IsArray({ message: "Images must be an array" })
  @IsString({ each: true, message: "Each image URL must be a string" })
  images?: string[];

  @ApiProperty({
    description: "Event details (optional)",
    example: "More information about the event...",
    required: false,
  })
  @IsOptional()
  @IsString({ message: "Details must be a string" })
  details?: string;
}
