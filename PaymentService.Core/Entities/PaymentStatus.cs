namespace PaymentService.Core.Entities;

public enum PaymentStatus
{
    Pending,        // Initial state, awaiting action or submission to gateway
    Processing,     // Submitted to gateway, awaiting final confirmation (e.g., webhook)
    Succeeded,      // Payment confirmed by gateway
    Failed,         // Payment failed at gateway or internally
    Cancelled,      // Payment explicitly cancelled by user or system before completion
    RequiresAction, // Payment requires further user action (e.g., 3DS, redirect)
    Refunded,       // Full amount refunded
    PartiallyRefunded // Part of the amount refunded
}