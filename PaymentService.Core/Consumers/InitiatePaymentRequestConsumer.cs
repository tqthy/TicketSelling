using MassTransit;
using Common.Messages; // For InitiatePaymentRequested
using PaymentService.Core.Services; // For IPaymentProcessingService
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using PaymentService.Core.Contracts.Gateways;

namespace PaymentService.Core.Consumers;

public class InitiatePaymentRequestConsumer : IConsumer<InitiatePaymentRequested>
{
    private readonly IPaymentProcessingService _paymentProcessingService;
    private readonly ILogger<InitiatePaymentRequestConsumer> _logger;
    private readonly IConfiguration _configuration;

    public InitiatePaymentRequestConsumer(
        IPaymentProcessingService paymentProcessingService,
        ILogger<InitiatePaymentRequestConsumer> logger,
        IConfiguration configuration)
    {
        _paymentProcessingService = paymentProcessingService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<InitiatePaymentRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received InitiatePaymentRequested event for BookingId: {BookingId}, Amount: {Amount} {Currency}, User: {UserId}, Gateway: {Gateway}",
            message.BookingId, message.Amount, message.Currency, message.UserId, message.PreferredGateway);

        try
        {
            // Map the message to the service request DTO
            // Note: The message from RabbitMQ might not have ReturnUrl/CancelUrl.
            // These might need to be configured in PaymentService or derived.
            // For now, let's assume they are configurable defaults or can be constructed.

            // TODO: Determine how to get ReturnUrl and CancelUrl.
            // Option 1: Configure them in PaymentService's appsettings.json
            // Option 2: If always the same pattern, construct them. e.g., "https://yourfrontend.com/payment/callback"
            // Option 3: If they need to be dynamic per booking, BookingService would need to provide them.
            // For this example, let's use placeholders. You'll need to replace these.
            string configuredReturnUrl = _configuration.GetSection("VnPay").GetSection("ReturnUrl").Value ?? "facebook.com";
            string configuredCancelUrl = $"https://your-domain.com/payment/cancel?bookingId={message.BookingId}";

            var serviceRequest = new CreatePaymentRequest
            {
                BookingId = message.BookingId,
                UserId = message.UserId,
                Amount = message.Amount,
                Currency = "VND", // Assuming VND is the currency for all payments
                OrderInfo = message.OrderInfo,
                PaymentGateway = message.PreferredGateway.ToString(), // Convert enum to string
                IpAddress = message.IpAddress,
                OrderType = message.OrderType,
                CreateDate = DateTime.UtcNow,
            };
                
            

            var initiationResult = await _paymentProcessingService.InitiatePaymentAsync(serviceRequest);

            // if (initiationResult.Success)
            // {
            //     _logger.LogInformation("Payment initiation successful for BookingId: {BookingId}. PaymentId: {PaymentId}, Gateway: {GatewayName}, Status: {Status}",
            //         message.BookingId, initiationResult.PaymentId, initiationResult.GatewayName, initiationResult.InitialStatus);
            //     // Further actions might be needed here, like:
            //     // - If a redirect URL is returned, how is the user redirected? (This is tricky with async, usually for API-driven flows)
            //     // - If a client secret is returned, it needs to get to the frontend.
            //     // This async flow is best suited for backend-to-backend communication where PaymentService handles everything,
            //     // or if the gateway supports a fully server-side flow without immediate user redirection.
            //     // If redirection IS needed, BookingService might need to expose an endpoint for the client to poll
            //     // for the redirect URL after initiating booking.
            // }
            // else
            // {
            //     _logger.LogError("Payment initiation failed for BookingId: {BookingId}. Reason: {Error}",
            //         message.BookingId, initiationResult.ErrorMessage);
            //     // Consider publishing a "PaymentInitiationFailed" event if other services need to know.
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InitiatePaymentRequested for BookingId: {BookingId}", message.BookingId);
            // MassTransit will handle retry/dead-lettering based on configuration.
            throw; // Re-throw to allow MassTransit to handle it
        }
    }
}