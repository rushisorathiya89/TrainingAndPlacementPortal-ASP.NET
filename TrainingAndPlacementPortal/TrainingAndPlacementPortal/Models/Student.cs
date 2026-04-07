using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingAndPlacementPortal.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnrollmentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Branch { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string? PersonalEmail { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        // Academic Details
        [Required]
        public double TenthPercentage { get; set; }
        
        [Required]
        public double TwelfthPercentage { get; set; }
        
        [Required]
        public double CGPA { get; set; }

        // Parent Information
        [MaxLength(100)]
        public string? ParentName { get; set; }
        [MaxLength(100)]
        [EmailAddress]
        public string? ParentEmail { get; set; }
        [MaxLength(15)]
        public string? ParentMobile { get; set; }

        // Address
        [MaxLength(200)]
        public string? CurrentAddress { get; set; }
        [MaxLength(200)]
        public string? PermanentAddress { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? State { get; set; }
        [MaxLength(10)]
        public string? Pincode { get; set; }

        // Documents paths
        public string? ConsentFormPath { get; set; }
        public string? PaymentProofPath { get; set; }

        public bool IsApproved { get; set; } = false;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
