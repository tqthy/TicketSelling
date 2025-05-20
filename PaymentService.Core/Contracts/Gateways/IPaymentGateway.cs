using Microsoft.AspNetCore.Http;

namespace PaymentService.Core.Contracts.Gateways;

public interface IPaymentGateway
{
    public Task<string> CreatePaymentUrl(CreatePaymentRequest serviceRequest);
    public Task HandleWebhookResult(HttpContext httpContext);
    public Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest);
}