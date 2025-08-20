using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RinhaBackend.Models;
using RinhaBackend.Services;

namespace RinhaBackend.Controllers
{
    [Route("/")]
    [ApiController]
    public class Payments : ControllerBase
    {
        private PaymentsServices _paymentsServices { get; set; }

        public Payments(PaymentsServices paymentsServices)
        {
            _paymentsServices = paymentsServices;
        }

        [HttpPost("payments")]
        public async Task<IActionResult> Post([FromBody] PaymentRequest paymentRequest)
        {
            await _paymentsServices.ProcessPayment(paymentRequest.correlationId, paymentRequest.amount);
            return Ok("Payment Received");
        }
        
        [HttpGet("payments-summary")]
        public IActionResult GetPaymentsSummary(DateTime from, DateTime to)
        {
            var summary = _paymentsServices.GetPaymentsSummary(from, to);
            return Ok(new
            {
                @default = summary.defaultData,
                fallback = summary.fallbackData
            });
        }
        
    }
}
