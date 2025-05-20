using Microsoft.AspNetCore.Mvc;
using PaymentService.Core.Services;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentWebhooksController(
    ILogger<PaymentWebhooksController> logger,
    IPaymentProcessingService paymentService)
    : ControllerBase
{
    [HttpGet("vnpay")]
    public async Task<IActionResult> VnPayWebhook()
    {
        logger.LogInformation("Received VnPay webhook");
        await paymentService.HandleWebhookResult(HttpContext, "VnPay");
        return Ok();
    }
    
}