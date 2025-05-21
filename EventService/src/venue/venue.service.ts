import { Injectable, HttpException, HttpStatus } from "@nestjs/common";
import { HttpService } from "@nestjs/axios";
import { LoggerService } from "../common/services/logger.service";
import { catchError, lastValueFrom, map, retry, timeout } from "rxjs";
import { AxiosError } from "axios";
import { SeatDto } from "./dto/seat.dto";

@Injectable()
export class VenueService {
  private readonly logger = new LoggerService(VenueService.name);
  constructor(private readonly httpService: HttpService) {
    this.logger.log("VenueService initialized");
  }

  async getAllSeatsByVenue(venueId: string): Promise<SeatDto[]> {
    if (!venueId) {
      this.logger.error(
        "Invalid venueId provided: venueId is empty or undefined"
      );
      return [];
    }

    this.logger.log(`Fetching all seats for venue ${venueId}`);

    try {
      const response = this.httpService
        .get<SeatDto[]>(`api/venues/${venueId}/seats`)
        .pipe(
          timeout(5000),
          retry({
            count: 3,
            delay: 1000,
            resetOnSuccess: true,
          }),
          map((response) => {
            this.logger.log(
              `Successfully fetched ${response.data.length} seats for venue ${venueId}`
            );
            return response.data;
          }),
          catchError((error: AxiosError) => {
            const statusCode =
              error.response?.status || HttpStatus.SERVICE_UNAVAILABLE;
            const errorMessage = error.response?.data
              ? JSON.stringify(error.response.data)
              : error.message;

            this.logger.error(
              `Failed to fetch seats from VenueService: ${errorMessage}`,
              error.stack
            );

            if (statusCode === HttpStatus.NOT_FOUND) {
              this.logger.warn(
                `Venue with ID ${venueId} not found in VenueService`
              );
              return [];
            }

            throw new HttpException(
              `Failed to fetch seats from VenueService: ${errorMessage}`,
              statusCode
            );
          })
        );

      return await lastValueFrom(response);
    } catch (error) {
      this.logger.error(
        `Error in getAllSeatsByVenue for venue ${venueId}: ${error.message}`,
        error.stack
      );

      return [];
    }
  }
}
