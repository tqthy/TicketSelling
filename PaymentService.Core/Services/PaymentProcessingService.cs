using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;
using PaymentService.Core.Exceptions;
using PaymentService.Core.Gateways;

namespace PaymentService.Core.Services;

public class PaymentProcessingService : IPaymentProcessingService
{
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentProcessingService> _logger;

    public PaymentProcessingService(
        IPaymentGatewayFactory gatewayFactory,
        IPaymentRepository paymentRepository,
        ILogger<PaymentProcessingService> logger)
    {
        _gatewayFactory = gatewayFactory ?? throw new ArgumentNullException(nameof(gatewayFactory));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> InitiatePaymentAsync(CreatePaymentRequest serviceRequest)
    {
        if (serviceRequest == null)
            throw new ArgumentNullException(nameof(serviceRequest));

        try
        {
            _logger.LogInformation("Initiating payment for booking {BookingId}", serviceRequest.BookingId);
            
            // Get the appropriate payment gateway
            var gateway = _gatewayFactory.GetGateway(serviceRequest.PaymentGateway);
            
            // Create payment record in database
            var payment = new Payment(
                bookingId: serviceRequest.BookingId,
                userId: serviceRequest.UserId,
                amount: serviceRequest.Amount,
                currency: serviceRequest.Currency ?? "VND",
                initialGatewayName: serviceRequest.PaymentGateway);
            
            await _paymentRepository.AddAsync(payment);
            
            // Generate payment URL
            var paymentUrl = await gateway.CreatePaymentUrl(serviceRequest);
            
            // Update payment with the payment URL
            // await _paymentRepository.UpdateAsync(payment);
            
            _logger.LogInformation("Successfully initiated payment {PaymentId} for booking {BookingId}", 
                payment.Id, payment.BookingId);
                
            return paymentUrl;
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogError(ex, "Payment gateway error while processing payment for booking {BookingId}", 
                serviceRequest.BookingId);
            throw new PaymentProcessingException(
                $"Payment processing failed: {ex.Message}", 
                "PAYMENT_PROCESSING_ERROR",
                ex.IsTransient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing payment for booking {BookingId}", 
                serviceRequest.BookingId);
            throw new PaymentProcessingException(
                "An unexpected error occurred while processing your payment. Please try again later.",
                "UNEXPECTED_ERROR");
        }
    }
    
    public async Task HandleWebhookResult(HttpContext httpContext, string paymentGateway)
    {
        if (httpContext == null)
            throw new ArgumentNullException(nameof(httpContext));
            
        if (string.IsNullOrWhiteSpace(paymentGateway))
            throw new ArgumentException("Payment gateway name is required.", nameof(paymentGateway));
            
        try
        {
            _logger.LogInformation("Processing webhook from {PaymentGateway}", paymentGateway);
            
            // Get the appropriate payment gateway
            var gateway = _gatewayFactory.GetGateway(paymentGateway);
            
            // Let the gateway handle the webhook
            await gateway.HandleWebhookResult(httpContext);
            
            _logger.LogInformation("Successfully processed webhook from {PaymentGateway}", paymentGateway);
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogError(ex, "Error processing webhook from {PaymentGateway}", paymentGateway);
            throw new WebhookProcessingException(
                $"Failed to process webhook: {ex.Message}",
                "WEBHOOK_PROCESSING_ERROR"
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook from {PaymentGateway}", paymentGateway);
            throw new WebhookProcessingException(
                "An unexpected error occurred while processing the webhook.",
                "UNEXPECTED_ERROR",
                true);
        }
    }
}