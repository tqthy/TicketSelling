import {
  Column,
  Entity,
  OneToMany,
  PrimaryGeneratedColumn,
  CreateDateColumn,
  UpdateDateColumn,
} from "typeorm";
import { EventSectionPricing } from "./event-section-pricing.entity";

export enum EventStatus {
  DRAFT = "Draft",
  SUBMIT_FOR_APPROVAL = "Submit for approval",
  PUBLISHED = "Published",
  POSTPONED = "Postponed",
  RESCHEDULED = "Rescheduled",
  CANCELED = "Canceled",
}

@Entity("EVENTS")
export class Event {
  @PrimaryGeneratedColumn("uuid")
  eventId: string;

  @Column()
  name: string;

  @Column()
  description: string;

  @Column()
  category: string;

  @Column({ type: "timestamp" })
  startDateTime: Date;

  @Column({ type: "timestamp" })
  endDateTime: Date;

  @Column({
    type: "enum",
    enum: EventStatus,
    default: EventStatus.DRAFT,
  })
  status: EventStatus;

  @Column()
  venueId: string;

  @Column()
  venueName: string;

  @Column()
  venueAddress: string;

  @Column()
  organizerUserId: string;

  @Column()
  poster: string;

  @Column({ type: "simple-array", nullable: true })
  images: string[];

  @Column({ type: "text", nullable: true })
  details: string;

  @CreateDateColumn()
  createdAt: Date;

  @UpdateDateColumn()
  updatedAt: Date;

  @OneToMany(() => EventSectionPricing, (pricing) => pricing.event)
  sectionPricing: EventSectionPricing[];
}
