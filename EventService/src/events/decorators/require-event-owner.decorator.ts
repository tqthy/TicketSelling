import { SetMetadata } from "@nestjs/common";

export const REQUIRE_EVENT_OWNER_KEY = "requireEventOwner";
export const RequireEventOwner = () =>
  SetMetadata(REQUIRE_EVENT_OWNER_KEY, true);
