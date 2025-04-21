import { IsNotEmpty, IsNumber, IsUUID, Min } from "class-validator";
import { ApiProperty } from "@nestjs/swagger";
import { Type, Transform } from "class-transformer";

export class EventSectionPricingDto {
  @ApiProperty({
    description: "Section ID",
    example: "123e4567-e89b-12d3-a456-426614174000",
  })
  @IsNotEmpty({ message: "Section ID is required" })
  @IsUUID("all", { message: "Section ID must be a valid UUID" })
  sectionId: string;

  @ApiProperty({ description: "Price for the section", example: 150.0 })
  @IsNotEmpty({ message: "Price is required" })
  @IsNumber(
    { maxDecimalPlaces: 2 },
    { message: "Price must be a number with at most 2 decimal places" }
  )
  @Min(0, { message: "Price must be a non-negative number" })
  @Transform(({ value }) => parseFloat(value))
  @Type(() => Number)
  price: number;
}
