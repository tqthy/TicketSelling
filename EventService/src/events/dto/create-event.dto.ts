import {
  IsArray,
  IsDateString,
  IsEnum,
  IsNotEmpty,
  IsOptional,
  IsString,
  IsUUID,
  IsUrl,
  ValidateNested,
  IsPositive,
} from "class-validator";
import {
  IsFutureDate,
  IsAfterField,
  IsTimeAfter,
} from "../../common/validators/date-validators";
// Status is managed internally
import { Type } from "class-transformer";
import { EventSectionPricingDto } from "./event-section-pricing.dto";
import { ApiProperty } from "@nestjs/swagger";

export class CreateEventDto {
  @ApiProperty({
    description: "Event name",
    example: "Bien Hoa FC vs. Dong Nai FC",
  })
  @IsNotEmpty({ message: "Event name is required" })
  @IsString({ message: "Event name must be a string" })
  name: string;

  @ApiProperty({
    description: "Event description",
    example: "A crucial match in the V.League",
  })
  @IsNotEmpty({ message: "Event description is required" })
  @IsString({ message: "Event description must be a string" })
  description: string;

  @ApiProperty({
    description: "Event category",
    enum: ["MATCH", "CONCERT", "OTHERS"],
    example: "MATCH",
  })
  @IsNotEmpty({ message: "Event category is required" })
  @IsEnum(["MATCH", "CONCERT", "OTHERS"], {
    message: "Category must be one of: MATCH, CONCERT, OTHERS",
  })
  category: "MATCH" | "CONCERT" | "OTHERS";

  @ApiProperty({
    description: "Event date (YYYY-MM-DD)",
    example: "2025-04-03",
  })
  @IsNotEmpty({ message: "Date is required" })
  @IsString({ message: "Date must be a string in format YYYY-MM-DD" })
  @IsFutureDate({ message: "Event date must be in the future" })
  date: string;

  @ApiProperty({
    description: "Event start time (HH:MM)",
    example: "19:30",
  })
  @IsNotEmpty({ message: "Start time is required" })
  @IsString({ message: "Start time must be a string in format HH:MM" })
  startTime: string;

  @ApiProperty({
    description: "Event end time (HH:MM)",
    example: "22:30",
  })
  @IsNotEmpty({ message: "End time is required" })
  @IsString({ message: "End time must be a string in format HH:MM" })
  @IsTimeAfter("startTime", "date", {
    message: "End time must be after start time",
  })
  endTime: string;

  @ApiProperty({
    description:
      "Event start date and time (auto-generated from date and startTime)",
    example: "2025-04-03T19:30:00.000Z",
    readOnly: true,
  })
  startDateTime?: string;

  @ApiProperty({
    description:
      "Event end date and time (auto-generated from date and endTime)",
    example: "2025-04-03T22:30:00.000Z",
    readOnly: true,
  })
  endDateTime?: string;

  @ApiProperty({
    description: "Venue ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  @IsNotEmpty({ message: "Venue ID is required" })
  @IsUUID("all", { message: "Venue ID must be a valid UUID" })
  venueId: string;

  @ApiProperty({ description: "Venue name", example: "Can Tho Stadium" })
  @IsNotEmpty({ message: "Venue name is required" })
  @IsString({ message: "Venue name must be a string" })
  venueName: string;

  @ApiProperty({
    description: "Venue address",
    example: "Cần Thơ, Phường Cái Khế, Quận Ninh Kiều, Thành Phố Cần Thơ",
  })
  @IsNotEmpty({ message: "Venue address is required" })
  @IsString({ message: "Venue address must be a string" })
  venueAddress: string;

  @ApiProperty({
    description: "Event poster URL",
    example: "https://example.com/poster.jpg",
  })
  @IsNotEmpty({ message: "Poster URL is required" })
  @IsString({ message: "Poster URL must be a string" })
  poster: string;

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

  @ApiProperty({
    description: "Section pricing information",
    type: [EventSectionPricingDto],
  })
  @IsArray({ message: "Section pricing must be an array" })
  @ValidateNested({ each: true })
  @Type(() => EventSectionPricingDto)
  sectionPricing: EventSectionPricingDto[];
}
