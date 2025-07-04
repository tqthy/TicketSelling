using Common.SeedWork;
using PaymentService.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentService.Core.Entities;

public class Payment : BaseEntity<Guid>, IAggregateRoot
{
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } // ISO currency code e.g. "USD"
    public PaymentStatus Status { get; private set; }
    public string? InitialGatewayName { get; private set; } // The first gateway this payment was attempted with

    // This would be the *final* successful transaction ID, or the latest significant one.
    // Individual attempt transaction IDs are in PaymentAttempt.
    public string? PrimaryGatewayTransactionId { get; set; }

    public string? FailureReason { get; private set; }
    public DateTime? LastSucceededAt { get; private set; }
    public DateTime? LastFailedAt { get; private set; }

    private readonly List<PaymentAttempt> _attempts = new();
    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    // Parameterless constructor for EF Core
    private Payment() : base(Guid.NewGuid()) { }

    public Payment(Guid bookingId, Guid userId, decimal amount, string currency, string initialGatewayName)
        : base(Guid.NewGuid()) // Generates a new PaymentId
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency));
        if (string.IsNullOrWhiteSpace(initialGatewayName))
            throw new ArgumentNullException(nameof(initialGatewayName));

        BookingId = bookingId;
        UserId = userId;
        Amount = amount;
        Currency = currency.ToUpper();
        InitialGatewayName = initialGatewayName;
        Status = PaymentStatus.Pending; // Initial status
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAttempt(PaymentAttempt attempt)
    {
        if (attempt.PaymentId != Id)
            throw new ArgumentException("PaymentAttempt belongs to a different Payment.", nameof(attempt));

        _attempts.Add(attempt);
        UpdatedAt = DateTime.UtcNow;

        // Update overall payment status based on the latest attempt
        if (attempt.WasSuccessful)
        {
            MarkAsSucceeded(attempt.GatewayTransactionId, attempt.GatewayName ?? InitialGatewayName!); // Requires gateway name
        }
        else
        {
            // If the payment isn't already succeeded, reflect the failure.
            // Avoid overwriting a Succeeded status with a later failed attempt (e.g., a failed refund attempt on a succeeded payment).
            // This logic might need refinement based on how refunds are handled.
            if (Status != PaymentStatus.Succeeded && Status != PaymentStatus.Refunded && Status != PaymentStatus.PartiallyRefunded)
            {
                MarkAsFailed(attempt.ErrorMessage ?? "Payment attempt failed.", attempt.GatewayTransactionId, attempt.GatewayName ?? InitialGatewayName!);
            }
        }
    }

    public void MarkAsProcessing(string gatewayName, string? gatewayTransactionId = null)
    {
        if (Status == PaymentStatus.Pending || Status == PaymentStatus.RequiresAction)
        {
            Status = PaymentStatus.Processing;
            PrimaryGatewayTransactionId = gatewayTransactionId ?? PrimaryGatewayTransactionId; // Keep existing if new one isn't provided
            FailureReason = null;
            UpdatedAt = DateTime.UtcNow;
            // Consider adding a PaymentAttempt here or ensuring one is added by the caller
        }
        else
        {
            throw new PaymentStateException($"Cannot transition to Processing from status {Status}.");
        }
    }
    
    public void MarkAsRequiresAction(string gatewayName, string? gatewayTransactionId = null)
    {
        if (Status == PaymentStatus.Pending || Status == PaymentStatus.Processing)
        {
            Status = PaymentStatus.RequiresAction;
            PrimaryGatewayTransactionId = gatewayTransactionId ?? PrimaryGatewayTransactionId;
            FailureReason = null;
            UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            throw new PaymentStateException($"Cannot transition to RequiresAction from status {Status}.");
        }
    }


    public void MarkAsSucceeded(string? gatewayTransactionId, string gatewayNameUsed)
    {
        // Allow transition from Pending, Processing, or RequiresAction
        if (Status == PaymentStatus.Pending || Status == PaymentStatus.Processing || Status == PaymentStatus.RequiresAction)
        {
            Status = PaymentStatus.Succeeded;
            PrimaryGatewayTransactionId = gatewayTransactionId ?? PrimaryGatewayTransactionId;
            FailureReason = null;
            LastSucceededAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            // AddDomainEvent(new PaymentSucceededDomainEvent(Id, BookingId, gatewayTransactionId));
        }
        else if (Status == PaymentStatus.Succeeded && PrimaryGatewayTransactionId == gatewayTransactionId)
        {
            // Idempotency: already marked as succeeded with the same transaction ID
            _attempts.LastOrDefault(a => a.GatewayTransactionId == gatewayTransactionId)?.SetAttemptTime(DateTime.UtcNow); // Update last attempt time if needed
        }
        else
        {
            throw new PaymentStateException($"Cannot transition to Succeeded from status {Status}. Current Gateway Tx ID: {PrimaryGatewayTransactionId}, Attempted: {gatewayTransactionId}");
        }
    }

    public void MarkAsFailed(string reason, string? gatewayTransactionId, string gatewayNameUsed)
    {
        if (Status == PaymentStatus.Succeeded || Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded)
        {
            // A payment that has already succeeded should not be marked as failed overall.
            // This might indicate a separate issue or a misunderstanding of the flow (e.g., a chargeback is different from a failed payment).
            throw new PaymentStateException($"Cannot mark an already '{Status}' payment as Failed.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        PrimaryGatewayTransactionId = gatewayTransactionId ?? PrimaryGatewayTransactionId;
        LastFailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        // AddDomainEvent(new PaymentFailedDomainEvent(Id, BookingId, reason));
    }

    public void MarkAsCancelled(string reason = "User cancelled")
    {
        if (Status == PaymentStatus.Pending || Status == PaymentStatus.RequiresAction || Status == PaymentStatus.Processing)
        {
            Status = PaymentStatus.Cancelled;
            FailureReason = reason; // Or a specific "Cancelled by user" reason
            UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            throw new PaymentStateException($"Cannot cancel payment from status {Status}.");
        }
    }

    // Placeholder for refund logic
    // public void ProcessRefund(decimal refundAmount, string reason) { ... }
}