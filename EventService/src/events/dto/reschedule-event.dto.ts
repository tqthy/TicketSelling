import { IsNotEmpty, IsString } from "class-validator";
import {
  IsFutureDate,
  IsDateTimeAfter,
} from "../../common/validators/date-validators";

export class RescheduleEventDto {
  @IsNotEmpty({ message: "New start date and time is required" })
  @IsString({ message: "New start date and time must be a string" })
  @IsFutureDate({
    message: "New event start date and time must be in the future",
  })
  newStartDateTime: string;

  @IsNotEmpty({ message: "New end date and time is required" })
  @IsString({ message: "New end date and time must be a string" })
  @IsDateTimeAfter("newStartDateTime", {
    message: "New end date and time must be after new start date and time",
  })
  newEndDateTime: string;
}
