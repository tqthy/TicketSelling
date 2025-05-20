using PaymentService.Core.Contracts.Gateways;

namespace PaymentService.Core.Services;

public interface IPaymentProcessingService
{
    Task<string> InitiatePaymentAsync(CreatePaymentRequest serviceRequest);
}