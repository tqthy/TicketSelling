using Microsoft.EntityFrameworkCore;
using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;

namespace PaymentService.Core.Persistence.Repositories;

public class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    // private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly PaymentDbContext _dbContext = dbContext;

    public void Add(Payment payment)
    {
        throw new NotImplementedException();
    }

    public void Update(Payment payment)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        await _dbContext.Set<Payment>().AddAsync(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _dbContext.Entry(payment).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<Payment?> GetByTransactionReferenceAsync(string transactionReference)
    {
        if (string.IsNullOrWhiteSpace(transactionReference))
            throw new ArgumentException("Transaction reference cannot be null or empty", nameof(transactionReference));
        
        // First try to find by PrimaryGatewayTransactionId
        var payment = await _dbContext.Set<Payment>()
            .FirstOrDefaultAsync(p => p.PrimaryGatewayTransactionId == transactionReference);
            
        if (payment != null)
            return payment;
            
        // If not found, try to find in the PaymentAttempts
        var paymentId = await _dbContext.Set<PaymentAttempt>()
            .Where(pa => pa.GatewayTransactionId == transactionReference)
            .Select(pa => pa.PaymentId)
            .FirstOrDefaultAsync();
            
        if (paymentId != Guid.Empty)
        {
            return await _dbContext.Set<Payment>()
                .Include(p => p.Attempts)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }
        
        return null;
    }
}