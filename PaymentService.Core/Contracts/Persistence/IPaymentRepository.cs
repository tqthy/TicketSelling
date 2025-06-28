using PaymentService.Core.Entities;

namespace PaymentService.Core.Contracts.Persistence;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    
    /// <summary>
    /// Gets a payment by its transaction reference
    /// </summary>
    /// <param name="transactionReference">The transaction reference to search for</param>
    /// <returns>The payment if found, otherwise null</returns>
    Task<Payment?> GetByTransactionReferenceAsync(string transactionReference);
}