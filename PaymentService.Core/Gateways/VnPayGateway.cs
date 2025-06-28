using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;
using PaymentService.Core.Exceptions;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace PaymentService.Core.Gateways;

public class VnPayGateway : BasePaymentGateway
{
    private readonly IVnpay _vnpay;
    public const string GatewayName = "VnPay";

    public VnPayGateway(
        IOptions<PaymentGatewayOptions> options,
        ILogger<VnPayGateway> logger)
        : base(GatewayName, options, logger)
    {
        // Validate required options
        if (string.IsNullOrWhiteSpace(Options.TmnCode))
            throw new PaymentGatewayConfigurationException("TmnCode is required for VNPay.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(Options.HashSecret))
            throw new PaymentGatewayConfigurationException("HashSecret is required for VNPay.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(Options.ReturnUrl))
            throw new PaymentGatewayConfigurationException("ReturnUrl is required for VNPay.", GatewayName);

        // Log the options being used (without sensitive data)
        logger.LogInformation("VnPayGateway initialized with options: {Options}", new {
            BaseUrl = !string.IsNullOrEmpty(Options.BaseUrl) ? "[SET]" : "[NOT SET]",
            TmnCode = !string.IsNullOrEmpty(Options.TmnCode) ? "[SET]" : "[NOT SET]",
            ReturnUrl = !string.IsNullOrEmpty(Options.ReturnUrl) ? "[SET]" : "[NOT SET]",
            Locale = Options.Locale ?? "[DEFAULT]"
        });
        
        _vnpay = new Vnpay();
        _vnpay.Initialize(
            Options.TmnCode, 
            Options.HashSecret, 
            Options.BaseUrl, 
            Options.ReturnUrl);
            
        Logger.LogInformation("Initialized VNPay gateway with TmnCode: {TmnCode}", Options.TmnCode);
    }
    

    public override async Task<string> CreatePaymentUrl(CreatePaymentRequest serviceRequest)
    {
        try
        {
            Logger.LogInformation("Creating payment URL for order: {OrderInfo}", serviceRequest.OrderInfo);
            
            ValidatePaymentRequest(serviceRequest);

            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = (double)serviceRequest.Amount,
                Description = serviceRequest.OrderInfo,
                IpAddress = serviceRequest.IpAddress,
                BankCode = BankCode.VNBANK,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese,
            };
            
            var paymentUrl = _vnpay.GetPaymentUrl(request);

            if (string.IsNullOrEmpty(paymentUrl))
                throw new PaymentGatewayException("Failed to generate payment URL.", "PAYMENT_URL_GENERATION_FAILED", GatewayName);

            Logger.LogInformation("Successfully created VNPay URL for order: {OrderInfo}", serviceRequest.OrderInfo);
            return paymentUrl;
        }
        catch (PaymentGatewayException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating VNPay URL for order: {OrderInfo}", serviceRequest.OrderInfo);
            throw new PaymentGatewayException(
                "An error occurred while creating payment URL. Please try again later.",
                "PAYMENT_URL_CREATION_ERROR",
                GatewayName,
                true, // Mark as transient
                ex);
        }
    }

    public override async Task HandleWebhookResult(HttpContext httpContext)
    {
        if (httpContext == null)
            throw new ArgumentNullException(nameof(httpContext));

        Logger.LogInformation("Processing VNPay webhook callback");
        
        try
        {
            // Extract and validate VNPay response parameters
            var vnpResponse = new VnPayResponse(httpContext.Request.Query);
            
            // Verify the response signature
            if (!vnpResponse.IsValid(Options.HashSecret!))
            {
                Logger.LogWarning("Invalid VNPay callback signature");
                throw new PaymentGatewayValidationException("Invalid payment callback signature", GatewayName, "INVALID_SIGNATURE");
            }

            // Get the payment repository from the service provider
            var serviceProvider = httpContext.RequestServices;
            var paymentRepository = serviceProvider.GetRequiredService<IPaymentRepository>();
            
            // Find the payment by the transaction reference (vnp_TxnRef)
            // Note: You might need to adjust this based on how you're storing the transaction reference
            var transactionRef = vnpResponse.OrderId;
            if (string.IsNullOrEmpty(transactionRef))
            {
                Logger.LogError("VNPay response is missing transaction reference (vnp_TxnRef)");
                throw new PaymentGatewayValidationException("Missing transaction reference", GatewayName, "MISSING_TRANSACTION_REF");
            }

            // In a real implementation, you would need to retrieve the payment by its reference
            // This is a simplified example - you'll need to implement the actual lookup logic
            var payment = await GetPaymentByReferenceAsync(transactionRef, paymentRepository);
            
            if (payment == null)
            {
                Logger.LogError("Payment not found for transaction reference: {TransactionRef}", transactionRef);
                throw new PaymentGatewayValidationException("Payment not found", GatewayName, "PAYMENT_NOT_FOUND");
            }

            

            // Process the payment status
            if (vnpResponse.ResponseCode == "00")
            {
                // Payment successful
                payment.MarkAsSucceeded(vnpResponse.TransactionId, GatewayName);
                Logger.LogInformation("VNPay payment successful for transaction: {TransactionId}", vnpResponse.TransactionId);
            }
            else
            {
                // Payment failed
                var errorMessage = $"VNPay payment failed. Response code: {vnpResponse.ResponseCode}, Message: {vnpResponse.Message}";
                payment.MarkAsFailed(errorMessage, vnpResponse.TransactionId, GatewayName);
                Logger.LogWarning("VNPay payment failed for transaction: {TransactionId}. {ErrorMessage}", 
                    vnpResponse.TransactionId, errorMessage);
            }

            
            // Create a new payment attempt
            var attempt = new PaymentAttempt(
                payment.Id,
                GatewayName,
                true,
                vnpResponse.TransactionId,
                vnpResponse.ResponseCode,
                vnpResponse.Message);
            // Add the attempt and update the payment
            payment.AddAttempt(attempt);
            await paymentRepository.UpdateAsync(payment);
            
            // Return a success response to VNPay
            await httpContext.Response.WriteAsJsonAsync(new 
            {
                RspCode = "00",
                Message = "Confirm success"
            });
        }
        catch (PaymentGatewayException ex)
        {
            Logger.LogError(ex, "Error processing VNPay webhook: {Message}", ex.Message);
            await httpContext.Response.WriteAsJsonAsync(new 
            {
                RspCode = "99",
                Message = $"Error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error processing VNPay webhook");
            await httpContext.Response.WriteAsJsonAsync(new 
            {
                RspCode = "99",
                Message = "Internal server error"
            });
        }
    }
    
    private async Task<Payment?> GetPaymentByReferenceAsync(string transactionRef, IPaymentRepository paymentRepository)
    {
        if (string.IsNullOrWhiteSpace(transactionRef))
        {
            Logger.LogWarning("Transaction reference is null or empty");
            return null;
        }

        try
        {
            // Use the repository to find the payment by transaction reference
            var payment = await paymentRepository.GetByTransactionReferenceAsync(transactionRef);
            
            if (payment == null)
            {
                Logger.LogWarning("Payment not found for transaction reference: {TransactionRef}", transactionRef);
            }
            
            return payment;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving payment for transaction reference: {TransactionRef}", transactionRef);
            throw new PaymentGatewayException(
                "An error occurred while retrieving payment information",
                "PAYMENT_RETRIEVAL_ERROR",
                GatewayName,
                true, // Mark as transient
                ex);
        }
    }

    public override Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest)
    {
        // TODO: Implement refund functionality
        throw new System.NotImplementedException("Refund functionality is not implemented yet.");
    }
}

// Helper class to parse and validate VNPay response
public class VnPayResponse
{
    private readonly IQueryCollection _query;

    public string? TransactionId => _query["vnp_TransactionNo"];
    public string? TransactionStatus => _query["vnp_TransactionStatus"];
    public string? ResponseCode => _query["vnp_ResponseCode"];
    public string? OrderId => _query["vnp_TxnRef"];

    public decimal Amount => !string.IsNullOrEmpty(_query["vnp_Amount"])
        ? decimal.Parse(_query["vnp_Amount"]) / 100
        : 0;

    public string? BankCode => _query["vnp_BankCode"];
    public string? Message => _query["vnp_Message"];
    public string? SecureHash => _query["vnp_SecureHash"];
    public string? SecureHashType => _query["vnp_SecureHashType"];
    public string? TxnResponseCode => _query["vnp_ResponseCode"];
    public string? TxnMessage => _query["vnp_Message"];

    public VnPayResponse(IQueryCollection query)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
    }

    public bool IsValid(string hashSecret)
    {
        if (string.IsNullOrEmpty(hashSecret))
            throw new ArgumentException("Hash secret cannot be null or empty", nameof(hashSecret));

        if (string.IsNullOrEmpty(SecureHash))
            return false;

        // Get all query parameters except vnp_SecureHash and vnp_SecureHashType
        var queryParams = _query
            .Where(kv => !string.IsNullOrEmpty(kv.Value) && 
                       !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                       !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        // Create the input string for hashing
        var signData = string.Join("&", queryParams
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

        // Compute the HMAC-SHA512 hash
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
        var computedHash = BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLower();

        // Compare the computed hash with the received hash
        return SecureHash.Equals(computedHash, StringComparison.OrdinalIgnoreCase);
    }
}