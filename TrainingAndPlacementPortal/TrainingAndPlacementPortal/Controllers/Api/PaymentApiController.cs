using Microsoft.AspNetCore.Mvc;
using TrainingAndPlacementPortal.Services;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentApiController : ControllerBase
    {
        private readonly RazorpayService _razorpayService;

        public PaymentApiController(RazorpayService razorpayService)
        {
            _razorpayService = razorpayService;
        }

        /// <summary>
        /// Creates a Razorpay order for student registration payment.
        /// Called by the frontend before opening the Razorpay Checkout popup.
        /// </summary>
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var receipt = $"reg_{request.EnrollmentNumber}_{DateTime.UtcNow.Ticks}";
                var order = _razorpayService.CreateOrder(_razorpayService.PaymentAmount, receipt);

                return Ok(new
                {
                    success = true,
                    orderId = order["id"].ToString(),
                    amount = (int)(_razorpayService.PaymentAmount * 100), // paise
                    currency = _razorpayService.Currency,
                    keyId = _razorpayService.KeyId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to create payment order.",
                    error = ex.Message
                });
            }
        }
    }

    public class CreateOrderRequest
    {
        public string EnrollmentNumber { get; set; } = string.Empty;
    }
}
