using System;
using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Website { get; set; }

        [MaxLength(100)]
        public string JobLocation { get; set; }

        public string Description { get; set; }

        // Contact Person
        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; }
        [Required]
        [MaxLength(15)]
        public string ContactMobile { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Verified, Rejected

        // Admin remarks for verification/rejection notes
        [MaxLength(500)]
        public string AdminRemarks { get; set; } = string.Empty;
        
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
