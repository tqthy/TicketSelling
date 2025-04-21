import { SetMetadata } from "@nestjs/common";

export const EVENT_OWNER_KEY = "requireEventOwner";
export const RequireEventOwner = () => SetMetadata(EVENT_OWNER_KEY, true);
