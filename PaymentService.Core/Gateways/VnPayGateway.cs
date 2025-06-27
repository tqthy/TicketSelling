using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
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
        if (string.IsNullOrWhiteSpace(Options.TmnCode))
            throw new PaymentGatewayConfigurationException("TmnCode is required for VNPay.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(Options.HashSecret))
            throw new PaymentGatewayConfigurationException("HashSecret is required for VNPay.", GatewayName);
            
        if (string.IsNullOrWhiteSpace(Options.ReturnUrl))
            throw new PaymentGatewayConfigurationException("ReturnUrl is required for VNPay.", GatewayName);

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
        try
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            Logger.LogInformation("Processing VNPay webhook callback");
            
            // Extract and validate VNPay response parameters
            var vnpResponse = new VnPayResponse(httpContext.Request.Query);
            
            // Verify the response signature
            if (!vnpResponse.IsValid(Options.HashSecret!))
            {
                Logger.LogWarning("Invalid VNPay callback signature");
                throw new PaymentGatewayValidationException("Invalid payment callback signature", GatewayName, "INVALID_SIGNATURE");
            }

            // Process the payment status
            if (vnpResponse.ResponseCode == "00")
            {
                // Payment successful
                // TODO: Update your payment record in the database
                Logger.LogInformation("VNPay payment successful for transaction: {TransactionId}", vnpResponse.TransactionId);
            }
            else
            {
                // Payment failed
                Logger.LogWarning("VNPay payment failed. Response code: {ResponseCode}, Message: {Message}", 
                    vnpResponse.ResponseCode, vnpResponse.Message);
                throw new PaymentGatewayException(
                    $"Payment failed. {vnpResponse.Message}",
                    vnpResponse.ResponseCode,
                    GatewayName);
            }
            
            // Return a success response to VNPay
            await httpContext.Response.WriteAsync("{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}");
        }
        catch (PaymentGatewayException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing VNPay webhook callback");
            throw new PaymentGatewayException(
                "An error occurred while processing payment callback. Please check the logs for details.",
                "CALLBACK_PROCESSING_FAILED",
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
        if (string.IsNullOrEmpty(SecureHash) || string.IsNullOrEmpty(hashSecret))
            return false;

        // TODO: Implement hash validation logic here
        // This should validate the SecureHash using the hashSecret
        // and the query parameters

        // For now, return true to indicate validation passed
        // In production, you should implement proper hash validation
        return true;
    }
}
                   