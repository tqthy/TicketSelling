using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.DTOs;
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
   
   public PaymentsController(ILogger<PaymentsController> logger, IConfiguration configuration, IMapper mapper, IPaymentProcessingService paymentProcessingService)
   {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
      _paymentProcessingService = paymentProcessingService;
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