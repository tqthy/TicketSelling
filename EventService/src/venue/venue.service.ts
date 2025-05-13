import { Injectable, HttpException, HttpStatus } from "@nestjs/common";
import { HttpService } from "@nestjs/axios";
import { LoggerService } from "../common/services/logger.service";
import { catchError, lastValueFrom, map } from "rxjs";
import { AxiosError } from "axios";
import { SeatDto } from "./dto/seat.dto";

@Injectable()
export class VenueService {
  private readonly logger = new LoggerService(VenueService.name);

  constructor(private readonly httpService: HttpService) {}

  async getAllSeatsByVenue(venueId: string): Promise<SeatDto[]> {
    this.logger.log(`Fetching all seats for venue ${venueId}`);

    try {
      const response = this.httpService
        .get<SeatDto[]>(`api/venues/${venueId}/seats`)
        .pipe(
          map((response) => response.data),
          catchError((error: AxiosError) => {
            throw new HttpException(
              `Failed to fetch seats from VenueService: ${error.message}`,
              HttpStatus.SERVICE_UNAVAILABLE
            );
          })
        );

      return await lastValueFrom(response);
    } catch (error) {
      throw error;
    }
  }
}
