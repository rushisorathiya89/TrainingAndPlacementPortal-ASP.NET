using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.DTOs
{
    public class RegisterStudentDto
    {
        [Required]
        [MaxLength(50)]
        public string EnrollmentNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Branch { get; set; }

        [Required]
        [MaxLength(20)]
        public string Semester { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }  // University email - used for login

        [EmailAddress]
        [MaxLength(100)]
        public string PersonalEmail { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        // Academic Details
        [Required]
        public double TenthPercentage { get; set; }

        [Required]
        public double TwelfthPercentage { get; set; }

        [Required]
        public double CGPA { get; set; }

        // Parent Information
        [MaxLength(100)]
        public string ParentName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string ParentEmail { get; set; }

        [MaxLength(15)]
        public string ParentMobile { get; set; }

        // Address
        [MaxLength(200)]
        public string CurrentAddress { get; set; }

        [MaxLength(200)]
        public string PermanentAddress { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(10)]
        public string Pincode { get; set; }
    }
}
