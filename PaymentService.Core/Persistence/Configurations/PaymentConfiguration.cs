using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Core.Entities;

namespace PaymentService.Core.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Table name
        builder.ToTable("Payments");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // Since we're generating GUIDs
            
        builder.Property(e => e.BookingId)
            .IsRequired();
            
        builder.Property(e => e.UserId)
            .IsRequired();
            
        builder.Property(e => e.Amount)
            .IsRequired();
            
        builder.Property(e => e.Currency)
            .HasMaxLength(3)
            .IsRequired();
            
        builder.Property(e => e.Status)
            .IsRequired();
            
        builder.Property(e => e.InitialGatewayName)
            .HasMaxLength(100);
            
        builder.Property(e => e.PrimaryGatewayTransactionId)
            .HasMaxLength(255);
            
        builder.Property(e => e.FailureReason)
            .HasMaxLength(1000);
        
        // Indexes
        builder.HasIndex(e => e.BookingId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        
        // Relationships
        builder.HasMany(e => e.Attempts)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}