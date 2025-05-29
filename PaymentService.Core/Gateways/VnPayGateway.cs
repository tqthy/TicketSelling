using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Services;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPay.NetCore;

namespace PaymentService.Core.Gateways;

public class VnPayGateway : IPaymentGateway
{
    // private static readonly Lazy<VnPayGateway> _instance = new(() => new VnPayGateway());
    private static IConfiguration _configuration = null!;
    private static ILogger<PaymentProcessingService> _logger = null!;

    // private VnPayGateway()
    // {
    //     // Private constructor to prevent instantiation
    // }
    
    private readonly IVnpay _vnpay;
    public VnPayGateway(IConfiguration configuration, ILogger<PaymentProcessingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var vnPayConfig = configuration.GetSection("VnPay");
        string vnp_Returnurl = vnPayConfig["ReturnUrl"] ?? throw new InvalidOperationException("ReturnUrl is missing in configuration.");
        string vnp_Url = vnPayConfig["BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is missing in configuration.");
        string vnp_TmnCode = vnPayConfig["TmnCode"] ?? throw new InvalidOperationException("TmnCode is missing in configuration.");
        string vnp_HashSecret = vnPayConfig["HashSecret"] ?? throw new InvalidOperationException("HashSecret is missing in configuration.");

        _vnpay = new Vnpay();
        _vnpay.Initialize(vnp_TmnCode, vnp_HashSecret, vnp_Url, vnp_Returnurl);
    }
    // public VnPayGateway(IConfiguration configuration, ILogger<PaymentProcessingService> logger)
    // {
    //     _configuration = configuration;
    //     _logger = logger;
    //     var vnPayConfig = _configuration.GetSection("VnPay");
    //     string vnp_Returnurl = vnPayConfig.GetSection("ReturnUrl").Value ?? "facebook.com";
    //     string vnp_Url = vnPayConfig.GetSection("BaseUrl").Value ?? "facebook.com";
    //     string vnp_TmnCode = vnPayConfig.GetSection("TmnCode").Value ?? "facebook.com";
    //     string vnp_HashSecret = vnPayConfig.GetSection("HashSecret").Value ?? "facebook.com";
    //     _vnpay = new Vnpay();
    //     _vnpay.Initialize(vnp_TmnCode, vnp_HashSecret, vnp_Url, vnp_Returnurl);
    // }
    //
    // public static VnPayGateway GetInstance(IConfiguration configuration, ILogger<PaymentProcessingService> logger)
    // {
    //     _configuration = configuration;
    //     _logger = logger;
    //     return _instance.Value;
    // }

    public Task<string> CreatePaymentUrl(CreatePaymentRequest serviceRequest)
    {
        if (_vnpay == null)
        {
            throw new InvalidOperationException("VNPay is not initialized.");
        }
        var request = new PaymentRequest
        {
            PaymentId = DateTime.Now.Ticks,
            Money = (double)serviceRequest.Amount,
            Description = serviceRequest.OrderInfo,
            IpAddress = "127.0.0.1",
            BankCode = BankCode.VNBANK, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
            CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
            Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
            Language = DisplayLanguage.Vietnamese,
        };
        
        var paymentUrl = _vnpay.GetPaymentUrl(request);

        _logger.LogInformation("VNPAY URL: {paymentUrl}", paymentUrl);
        return Task.FromResult(paymentUrl);
    }
    // public Task<string> CreatePaymentUrl(CreatePaymentRequest serviceRequest)
    // {
    //     var vnPayConfig = _configuration.GetSection("VnPay");
    //     string vnp_Returnurl = vnPayConfig.GetSection("ReturnUrl").Value ?? "facebook.com";
    //     string vnp_Url = vnPayConfig.GetSection("BaseUrl").Value ?? "facebook.com";
    //     string vnp_TmnCode = vnPayConfig.GetSection("TmnCode").Value ?? "facebook.com";
    //     string vnp_HashSecret = vnPayConfig.GetSection("HashSecret").Value ?? "facebook.com";
    //     string locale = vnPayConfig.GetSection("Locale").Value ?? "vn";
    //     
    //     _logger.LogInformation($"VnPay Configuration: ReturnUrl={vnp_Returnurl}, BaseUrl={vnp_Url}, TmnCode={vnp_TmnCode}, HashSecret={vnp_HashSecret}, Locale={locale}");
    //     
    //     if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
    //     {
    //         throw new Exception("VNPAY configuration is missing.");
    //     }
    //     //Build URL for VNPAY
    //     VnPayLibrary vnpay = new();
    //     vnpay.AddRequestData("vnp_Version", "2.1.1");
    //     vnpay.AddRequestData("vnp_Command", "pay");
    //     vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
    //     vnpay.AddRequestData("vnp_Amount", (serviceRequest.Amount * 100).ToString());
    //     vnpay.AddRequestData("vnp_BankCode", "VNBANK");
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
    //     vnpay.AddRequestData("vnp_OrderInfo", serviceRequest.OrderInfo);
    //     vnpay.AddRequestData("vnp_OrderType", serviceRequest.OrderType);
    //     vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
    //     vnpay.AddRequestData("vnp_TxnRef", serviceRequest.BookingId.ToString());
    //     //Add Params of 2.1.0 Version
    //     vnpay.AddRequestData("vnp_ExpireDate", serviceRequest.ExpireDate.ToString("yyyyMMddHHmmss"));
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
    //     var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
    //         
    //     _logger.LogInformation("VNPAY URL: {paymentUrl}", paymentUrl);
    //     
    //     return Task.FromResult(paymentUrl);
    // }

    public Task HandleWebhookResult(HttpContext context)
    {
        string returnContent = string.Empty;
        var vnPayConfig = _configuration.GetSection("VnPay");
        
            if (context.Request.Query.Count > 0)
            {
                string vnp_HashSecret = vnPayConfig.GetSection("HashSecret").Value ?? "facebook.com";
                var vnpayData = context.Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();
                var parametersDictionary = new Dictionary<string, string>();

                foreach (var s in vnpayData)
                {
                    var key = s.Key;
                    var value = s.Value;
                    if (key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, value.ToString());
                    }
                    parametersDictionary.Add(s.Key, s.Value.FirstOrDefault() ?? string.Empty);

                }
                //Lay danh sach tham so tra ve tu VNPAY
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount"))/100;
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = parametersDictionary["vnp_SecureHash"];
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        //Thanh toan thanh cong
                        _logger.LogInformation("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId,
                            vnpayTranId);
                        
                    } else
                    {
                        _logger.LogInformation("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}",
                            orderId, vnpayTranId, vnp_ResponseCode);
                    }
                }
                else
                {
                    _logger.LogInformation("Invalid signature, InputData={0}", context.Request.GetDisplayUrl());
                    returnContent = "{\"RspCode\":\"97\",\"Message\":\"Invalid signature\"}";
                }
            }
            else
            {
                returnContent = "{\"RspCode\":\"99\",\"Message\":\"Input data required\"}";
            }

            return Task.CompletedTask;
    }

    public Task<ProcessRefundResponse> ProcessRefund(ProcessRefundResponse serviceRequest)
    {
        throw new NotImplementedException();
    }
}