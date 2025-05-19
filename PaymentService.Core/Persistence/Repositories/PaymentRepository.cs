using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;

namespace PaymentService.Core.Persistence.Repositories;

public class PaymentRepository(IUnitOfWork unitOfWork, PaymentDbContext dbContext) : IPaymentRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly PaymentDbContext _dbContext = dbContext;

    public void Add(Payment payment)
    {
        
    }

    public void Update(Payment payment)
    {
        throw new NotImplementedException();
    }
}