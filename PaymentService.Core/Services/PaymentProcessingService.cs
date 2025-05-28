using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Contracts.Persistence;
using PaymentService.Core.Entities;
using PaymentService.Core.Gateways;
using VNPay.NetCore;

namespace PaymentService.Core.Services;


public class PaymentProcessingService(IPaymentGateway paymentGateway, IPaymentRepository paymentRepository, ILogger<PaymentProcessingService> logger, IConfiguration configuration)
    : IPaymentProcessingService
{
    private IPaymentGateway _paymentGateway = paymentGateway;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;

    public async Task<string> InitiatePaymentAsync(CreatePaymentRequest serviceRequest)
    {
        if (serviceRequest.PaymentGateway == "VnPay") _paymentGateway = VnPayGateway.GetInstance(configuration, logger);
        
        var paymentUrl = await _paymentGateway.CreatePaymentUrl(serviceRequest);

        return paymentUrl;
    }
    
    public async Task HandleWebhookResult(HttpContext httpContext, string paymentGateway)
    {
        if (paymentGateway == "VnPay") _paymentGateway = VnPayGateway.GetInstance(configuration, logger);
        
        
        await _paymentGateway.HandleWebhookResult(httpContext);
        
    }  
 
    
    // private async Task<string> GetVnPayPaymentUrl(CreatePaymentRequest serviceRequest)
    // {
    //     var vnPayConfig = configuration.GetSection("VnPay");
    //     string vnp_Returnurl = vnPayConfig.GetSection("ReturnUrl").Value ?? "facebook.com";
    //     string vnp_Url = vnPayConfig.GetSection("BaseUrl").Value ?? "facebook.com";
    //     string vnp_TmnCode = vnPayConfig.GetSection("TmnCode").Value ?? "facebook.com";
    //     string vnp_HashSecret = vnPayConfig.GetSection("HashSecret").Value ?? "facebook.com";
    //     string locale = vnPayConfig.GetSection("Locale").Value ?? "vn";
    //     if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
    //     {
    //         throw new Exception("VNPAY configuration is missing.");
    //     }
    //     //Get payment input
    //     // TODO: Save order to db
    //     // OrderInfo order = new OrderInfo();
    //     //
    //     // order.OrderId = DateTime.Now.Ticks;
    //     // order.Amount = 100000;
    //     // order.Status = "0"; 
    //     // order.OrderDesc = txtOrderDesc.Text;
    //     // order.CreatedDate = DateTime.Now;
    //     
    //     //Build URL for VNPAY
    //     VnPayLibrary vnpay = new();
    //     vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
    //     vnpay.AddRequestData("vnp_Command", "pay");
    //     vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
    //     vnpay.AddRequestData("vnp_Amount", (serviceRequest.Amount * 100).ToString());
    //     // vnpay.AddRequestData("vnp_BankCode", cboBankCode.SelectedItem.Value);
    //     vnpay.AddRequestData("vnp_CreateDate", serviceRequest.CreateDate.ToString("yyyyMMddHHmmss"));
    //     vnpay.AddRequestData("vnp_CurrCode", "VND");
    //     vnpay.AddRequestData("vnp_IpAddr", serviceRequest.IpAddress);
    //     if (!string.IsNullOrEmpty(locale))
    //     {
    //         vnpay.AddRequestData("vnp_Locale", locale);
    //     }
    //     else
    //     {
    //         vnpay.AddRequestData("vnp_Locale", "vn");
    //     }
    //     // vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + serviceRequest.OrderId);
    //     vnpay.AddRequestData("vnp_OrderType", serviceRequest.OrderType);
    //     vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
    //     // vnpay.AddRequestData("vnp_TxnRef", serviceRequest.OrderId.ToString());
    //     //Add Params of 2.1.0 Version
    //     vnpay.AddRequestData("vnp_ExpireDate", serviceRequest.CreateDate.AddMinutes(15).ToString("yyyyMMddHHmmss"));
    //     //Billing
    //     // vnpay.AddRequestData("vnp_Bill_Mobile", txt_billing_mobile.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Bill_Email", txt_billing_email.Text.Trim());
    //     // var fullName = txt_billing_fullname.Text.Trim();
    //     // if (!String.IsNullOrEmpty(fullName))
    //     // {
    //     //     var indexof = fullName.IndexOf(' ');
    //     //     vnpay.AddRequestData("vnp_Bill_FirstName", fullName.Substring(0, indexof));
    //     //     vnpay.AddRequestData("vnp_Bill_LastName", fullName.Substring(indexof + 1, 
    //     //     fullName.Length - indexof - 1));
    //     // }
    //     // vnpay.AddRequestData("vnp_Bill_Address", txt_inv_addr1.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Bill_City", txt_bill_city.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Bill_Country", txt_bill_country.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Bill_State", "");
    //     // Invoice
    //     // vnpay.AddRequestData("vnp_Inv_Phone", txt_inv_mobile.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Inv_Email", txt_inv_email.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Inv_Customer", txt_inv_customer.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Inv_Address", txt_inv_addr1.Text.Trim());
    //     // vnpay.AddRequestData("vnp_Inv_Company", txt_inv_company.Text);
    //     // vnpay.AddRequestData("vnp_Inv_Taxcode", txt_inv_taxcode.Text);
    //     // vnpay.AddRequestData("vnp_Inv_Type", cbo_inv_type.SelectedItem.Value);
    //
    //     string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
    //         
    //     logger.LogInformation("VNPAY URL: {paymentUrl}", paymentUrl);
    //     return paymentUrl;
    // }
}