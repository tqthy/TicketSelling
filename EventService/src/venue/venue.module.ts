import { Module } from "@nestjs/common";
import { HttpModule } from "@nestjs/axios";
import { ConfigModule, ConfigService } from "@nestjs/config";
import { VenueService } from "./venue.service";

@Module({
  imports: [
    HttpModule.registerAsync({
      imports: [ConfigModule],
      useFactory: (configService: ConfigService) => ({
        baseURL: configService.get("http://localhost:5110"),
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
