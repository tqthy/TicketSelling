using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Exceptions;

namespace PaymentService.Core.Gateways;

public abstract class BasePaymentGateway : IPaymentGateway
{
    protected readonly ILogger Logger;
    protected readonly string GatewayName;
    protected readonly PaymentGatewayOptions Options;

    protected BasePaymentGateway(
        string gatewayName,
        IOptions<PaymentGatewayOptions> options,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(gatewayName))
            throw new ArgumentException("Gateway name cannot be null or whitespace.", nameof(gatewayName));

        GatewayName = gatewayName;
        Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        ValidateConfiguration();
    }

    protected virtual void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(Options.BaseUrl))
            throw new PaymentGatewayConfigurationException("Base URL is not configured.", GatewayName);
    }

    public abstract Task<string> CreatePaymentUrl(CreatePaymentRequest serviceRequest);
    public abstract Task HandleWebhookResult(HttpContext httpContext);
    public abstract Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest);
    
    protected virtual void ValidatePaymentRequest(CreatePaymentRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        if (request.Amount <= 0)
            throw new PaymentGatewayValidationException("Amount must be greater than zero.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(request.OrderInfo))
            throw new PaymentGatewayValidationException("Order information is required.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(request.IpAddress))
            throw new PaymentGatewayValidationException("IP address is required.", GatewayName);
    }
}

public class PaymentGatewayOptions
{
    // public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string ReturnUrl { get; set; }
    public string HashSecret { get; set; }
    public string TmnCode { get; set; }
    public string Locale { get; set; } = "vn";
    public int TimeoutInSeconds { get; set; } = 30;
}
