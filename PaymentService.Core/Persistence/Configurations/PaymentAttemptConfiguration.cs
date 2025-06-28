using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Core.Entities;

namespace PaymentService.Core.Persistence.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        // Table name
        builder.ToTable("PaymentAttempts");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
            
        builder.Property(e => e.PaymentId)
            .IsRequired();
            
        builder.Property(e => e.AttemptedAt)
            .IsRequired();
            
        builder.Property(e => e.WasSuccessful)
            .IsRequired();
            
        builder.Property(e => e.GatewayName)
            .HasMaxLength(100);
            
        builder.Property(e => e.GatewayTransactionId)
            .HasMaxLength(255);
            
        builder.Property(e => e.GatewayResponseDetails)
            .HasMaxLength(4000);
            
        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);
        
        // Indexes
        builder.HasIndex(e => e.PaymentId);
        builder.HasIndex(e => e.AttemptedAt);
        builder.HasIndex(e => e.WasSuccessful);
        builder.HasIndex(e => e.GatewayName);
        builder.HasIndex(e => e.GatewayTransactionId);
    }
}