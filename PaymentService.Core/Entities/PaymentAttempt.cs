using Common.SeedWork;
using System;

namespace PaymentService.Core.Entities;

public class PaymentAttempt : BaseEntity<Guid>
{
    public Guid PaymentId { get; private set; }
    public DateTime AttemptedAt { get; private set; }
    public bool WasSuccessful { get; private set; }
    public string? GatewayName { get; private set; } // Which gateway was used for this attempt
    public string? GatewayTransactionId { get; private set; } // ID from the gateway for this specific attempt/intent
    public string? GatewayResponseDetails { get; private set; } // Store key details from gateway response for diagnostics
    public string? ErrorMessage { get; private set; } // If the attempt failed

    // Parameterless constructor for EF Core
    private PaymentAttempt() : base(Guid.NewGuid()) { }

    public PaymentAttempt(
        Guid paymentId,
        string gatewayName,
        bool wasSuccessful,
        string? gatewayTransactionId = null,
        string? gatewayResponseDetails = null,
        string? errorMessage = null) : base(Guid.NewGuid())
    {
        PaymentId = paymentId;
        GatewayName = gatewayName;
        AttemptedAt = DateTime.UtcNow;
        WasSuccessful = wasSuccessful;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponseDetails = gatewayResponseDetails;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow; // From BaseEntity
    }

    public void SetAttemptTime(DateTime newAttemptedAt)
    {
        AttemptedAt = newAttemptedAt;
        UpdatedAt = DateTime.UtcNow; // From BaseEntity
    }
    
}