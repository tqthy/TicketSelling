using BookingService.Domain.AggregatesModel.BookingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Data;

public class BookingEntityTypeConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Configure primary key
        builder.HasKey(b => b.Id);

        // Configure properties
        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.TotalPrice)
            .IsRequired();

        builder.Property(b => b.ExpiresAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasMany(b => b.BookedSeats)
            .WithOne(bs => bs.Booking)
            .HasForeignKey(bs => bs.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure table name (optional)
        builder.ToTable("Bookings");
    }
}