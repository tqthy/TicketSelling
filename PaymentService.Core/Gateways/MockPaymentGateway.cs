using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;
using PaymentService.Core.Exceptions;
using PaymentService.Core.Models;
using VNPAY.NET.Models;

namespace PaymentService.Core.Gateways
{
    public class MockPaymentGateway : BasePaymentGateway
    {
        private static readonly Random _random = new Random();
        private const string MockTransactionIdPrefix = "MOCK";

        public MockPaymentGateway(
            IOptions<PaymentGatewayOptions> options,
            ILogger<MockPaymentGateway> logger) 
            : base("Mock", options, logger)
        {
        }

        public override async Task<(string PaymentUrl, string TransactionId)> CreatePaymentUrl(CreatePaymentRequest serviceRequest)
        {
            // Generate a mock transaction ID
            var transactionId = $"{MockTransactionIdPrefix}{DateTime.UtcNow.Ticks}";
            
            // Create a mock payment URL that points to our own endpoint
            var callbackUrl = $"{Options.ReturnUrl}?paymentId={serviceRequest.OrderInfo}&transactionId={transactionId}";
            
            Logger.LogInformation("Created mock payment URL: {Url} with transaction ID: {TransactionId}", 
                callbackUrl, transactionId);
            
            return (callbackUrl, transactionId);
        }

        public override async Task HandleWebhookResult(HttpContext httpContext)
        {
            var query = httpContext.Request.Query;

            var transactionRef = query["transactionId"];
            var statusStr = query["status"];

            if (string.IsNullOrEmpty(transactionRef) || string.IsNullOrEmpty(statusStr))
            {
                Logger.LogWarning("Invalid mock payment webhook parameters");
                throw new PaymentGatewayValidationException("Invalid mock payment webhook parameters", GatewayName, "INVALID_PARAMETERS");
            }

            var serviceProvider = httpContext.RequestServices;
            var paymentRepository = serviceProvider.GetRequiredService<IPaymentRepository>();

            var payment = await paymentRepository.GetByTransactionReferenceAsync(transactionRef);

            if (payment == null)
            {
                Logger.LogError("Payment not found for transaction reference: {TransactionRef}", transactionRef);
                throw new PaymentGatewayValidationException("Payment not found", GatewayName, "PAYMENT_NOT_FOUND");
            }

            var status = Enum.TryParse<PaymentStatus>(statusStr, out var parsedStatus)
                ? parsedStatus
                : PaymentStatus.Failed;

            if (status == PaymentStatus.Succeeded)
            {
                // Payment successful
                payment.MarkAsSucceeded(transactionRef, GatewayName);
                Logger.LogInformation("MOCK payment successful for transaction: {TransactionId}", transactionRef);
            }
            else
            {
                // Payment failed
                var errorMessage = $"Mock payment failed.";
                payment.MarkAsFailed(errorMessage, transactionRef, GatewayName);
                Logger.LogWarning("Mock payment failed for transaction: {TransactionId}. {ErrorMessage}", 
                    transactionRef, errorMessage);
            }

            Logger.LogInformation("{Status} MOCK payment for transaction: {TransactionId}", status, transactionRef);

            var attempt = new PaymentAttempt(
                payment.Id,
                GatewayName,
                status == PaymentStatus.Succeeded,
                transactionRef);

            payment.AddAttempt(attempt);
            await paymentRepository.UpdateAsync(payment);

            // Return a success response 
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Confirm success"
            });
        }

        public override async Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest)
        {
            // For testing purposes, always return success
            return new ProcessRefundResponse();
            // return new ProcessRefundResponse
            // {
            //     Success = true,
            //     TransactionId = $"REFUND-{DateTime.UtcNow.Ticks}",
            //     ResponseCode = "00",
            //     ResponseMessage = "Mock refund processed successfully"
            // };
        }

        protected override void ValidateConfiguration()
        {
            // No specific configuration needed for mock gateway
        }
    }
}
