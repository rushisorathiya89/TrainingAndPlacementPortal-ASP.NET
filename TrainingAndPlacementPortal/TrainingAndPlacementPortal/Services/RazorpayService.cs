using System.Security.Cryptography;
using System.Text;
using Razorpay.Api;

namespace TrainingAndPlacementPortal.Services
{
    public class RazorpayService
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly decimal _paymentAmount;
        private readonly string _currency;

        public RazorpayService(IConfiguration configuration)
        {
            var settings = configuration.GetSection("RazorpaySettings");
            _keyId = settings["KeyId"]!;
            _keySecret = settings["KeySecret"]!;
            _paymentAmount = decimal.Parse(settings["PaymentAmount"] ?? "500");
            _currency = settings["Currency"] ?? "INR";
        }

        public string KeyId => _keyId;
        public decimal PaymentAmount => _paymentAmount;
        public string Currency => _currency;

        /// <summary>
        /// Creates a Razorpay order. Amount is in INR (will be converted to paise internally).
        /// </summary>
        public Order CreateOrder(decimal amountInRupees, string receipt)
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amountInRupees * 100) }, // Convert to paise
                { "currency", _currency },
                { "receipt", receipt },
                { "payment_capture", 1 } // Auto capture
            };

            return client.Order.Create(options);
        }

        /// <summary>
        /// Verifies the Razorpay payment signature using HMAC-SHA256.
        /// This ensures the payment response has not been tampered with.
        /// </summary>
        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            var text = orderId + "|" + paymentId;
            var key = Encoding.UTF8.GetBytes(_keySecret);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return generatedSignature == signature;
        }
    }
}
