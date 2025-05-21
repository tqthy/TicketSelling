import { Module } from "@nestjs/common";
import { HttpModule } from "@nestjs/axios";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { VenueService } from "./venue.service";

@Module({
  imports: [
    ConfigModule,
    HttpModule.registerAsync({
      imports: [ConfigModule],
      useFactory: () => ({
        baseURL: "http://venueservice:8083",
        timeout: 5000,
        maxRedirects: 5,
        headers: {
          Accept: "application/json",
        },
      }),
      inject: [ConfigService],
    }),
  ],
  providers: [VenueService],
  exports: [VenueService],
})
export class VenueModule {}
