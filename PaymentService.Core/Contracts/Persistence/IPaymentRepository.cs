using PaymentService.Core.Entities;

namespace PaymentService.Core.Contracts.Persistence;

public interface IPaymentRepository
{
    void Add(Payment payment);
    void Update(Payment payment);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}