import { IsNotEmpty, IsString } from "class-validator";
import { ApiProperty } from "@nestjs/swagger";
import {
  IsFutureDate,
  IsTimeAfter,
} from "../../common/validators/date-validators";

export class RescheduleEventDto {
  @ApiProperty({
    description: "New event date (YYYY-MM-DD)",
    example: "2025-04-10",
  })
  @IsNotEmpty({ message: "Date is required" })
  @IsString({ message: "Date must be a string in format YYYY-MM-DD" })
  @IsFutureDate({ message: "Event date must be in the future" })
  date: string;

  @ApiProperty({
    description: "New event start time (HH:MM)",
    example: "19:30",
  })
  @IsNotEmpty({ message: "Start time is required" })
  @IsString({ message: "Start time must be a string in format HH:MM" })
  startTime: string;

  @ApiProperty({
    description: "New event end time (HH:MM)",
    example: "22:30",
  })
  @IsNotEmpty({ message: "End time is required" })
  @IsString({ message: "End time must be a string in format HH:MM" })
  @IsTimeAfter("startTime", "date", {
    message: "End time must be after start time",
  })
  endTime: string;
}
