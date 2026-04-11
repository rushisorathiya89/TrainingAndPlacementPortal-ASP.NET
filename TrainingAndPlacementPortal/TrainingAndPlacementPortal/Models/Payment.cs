using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingAndPlacementPortal.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        [MaxLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [MaxLength(500)]
        public string? RazorpaySignature { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Created"; // Created, Paid, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
