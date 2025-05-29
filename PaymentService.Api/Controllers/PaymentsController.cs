using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.DTOs;
using PaymentService.Core.Contracts;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Services;
using VNPay.NetCore;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
   private readonly ILogger<PaymentsController> _logger;
   private readonly IConfiguration _configuration;
   private readonly IPaymentProcessingService _paymentProcessingService;
   private readonly IMapper _mapper;
   private readonly IBookingServiceClient _bookingServiceClient;
   public PaymentsController(ILogger<PaymentsController> logger, IConfiguration configuration, IMapper mapper, IPaymentProcessingService paymentProcessingService, IBookingServiceClient bookingServiceClient)
   {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
      _paymentProcessingService = paymentProcessingService;
      _bookingServiceClient = bookingServiceClient ?? throw new ArgumentNullException(nameof(bookingServiceClient));
   }
   
   /// <summary>
   /// This method simulates the creation of a payment.
   /// Used for MVP purposes, it does not interact with a real payment gateway.
   /// </summary>
   [HttpPost]
   public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request)
   {
      try
      {
         if (request == null)
         {
            return await Task.FromResult<IActionResult>(BadRequest("Invalid request data."));
         }

         // Validate required fields
         if (string.IsNullOrWhiteSpace(request.CardHolderName) ||
             string.IsNullOrWhiteSpace(request.ExpiryMonth) ||
             string.IsNullOrWhiteSpace(request.ExpiryYear) ||
             string.IsNullOrWhiteSpace(request.Cvv))
         {
            return await Task.FromResult<IActionResult>(BadRequest("Missing required card details."));
         }

         // Predefined valid card details
         var validCardDetails = new
         {
            CardNumbers = new List<string> { "371449635398431", "378734493671000", "378282246310005" },
            CardHolderName = "Duong Thuan Tri",
            ExpiryMonth = "12",
            ExpiryYear = "2029",
            Cvv = "512"
         };

         // Check if all card details are correct
         if (!validCardDetails.CardNumbers.Contains(request.CardNumber) ||
             request.CardHolderName != validCardDetails.CardHolderName ||
             request.ExpiryMonth != validCardDetails.ExpiryMonth ||
             request.ExpiryYear != validCardDetails.ExpiryYear ||
             request.Cvv != validCardDetails.Cvv)
         {
            return await Task.FromResult<IActionResult>(BadRequest("Card details are incorrect."));
         }

         // Simulate payment processing logic
         
         // Inform Booking Service about the payment success
         await _bookingServiceClient.UpdateBookingStatusAsync(request.BookingId, "Confirmed");
         
         CreatePaymentRequest createPaymentRequest = _mapper.Map<CreatePaymentRequest>(request);
         _logger.LogDebug("Creating payment request: {@CreatePaymentRequest}", createPaymentRequest);
         
         _logger.LogInformation("Payment initiated for card: {CardNumber}", request.CardNumber);
         return await Task.FromResult<IActionResult>(Ok("Payment successfully initiated."));
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error processing payment.");
         return await Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing the payment."));
      }
   }
   
   [HttpGet("vnpay")]
   public Task<IActionResult> GetVnPayUrl([FromQuery] GetVnPayUrlDto request)
   {
      try
      {
         if (request == null)
         {
            return Task.FromResult<IActionResult>(BadRequest("Invalid request data."));
         }

         CreatePaymentRequest createPaymentRequest = _mapper.Map<CreatePaymentRequest>(request);
         
         var url = _paymentProcessingService.InitiatePaymentAsync(createPaymentRequest).Result;

         return Task.FromResult<IActionResult>(Ok(new { PaymentUrl = url }));
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error generating VNPay URL.");
         return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while generating the payment URL."));
      }
   }
   
}