using Microsoft.AspNetCore.Http;

namespace PaymentService.Core.Contracts.Gateways;

public interface IPaymentGateway
{
    /// <summary>
    /// Creates a payment URL and returns a tuple containing the URL and the gateway transaction ID
    /// </summary>
    public Task<(string PaymentUrl, string TransactionId)> CreatePaymentUrl(CreatePaymentRequest serviceRequest);
    
    public Task HandleWebhookResult(HttpContext httpContext);
    public Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest);
}