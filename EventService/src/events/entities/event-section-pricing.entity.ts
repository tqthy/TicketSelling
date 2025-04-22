import { Column, Entity, JoinColumn, ManyToOne, PrimaryGeneratedColumn } from 'typeorm';
import { Event } from './event.entity';

@Entity('EVENT_SECTION_PRICING')
export class EventSectionPricing {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column()
  eventId: string;

  @Column()
  sectionId: string;

  @Column('decimal', { precision: 10, scale: 2 })
  price: number;

  @ManyToOne(() => Event, (event) => event.sectionPricing)
  @JoinColumn({ name: 'eventId' })
  event: Event;
}
