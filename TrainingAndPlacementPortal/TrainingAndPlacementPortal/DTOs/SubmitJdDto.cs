using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.DTOs
{
    public class SubmitJdDto
    {
        // Company Details
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(100)]
        public string? JobLocation { get; set; }

        public string? Description { get; set; }

        // Job Role & Compensation
        [Required]
        [MaxLength(100)]
        public string JobPosition { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AnnualCTC { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? InternshipDuration { get; set; }

        [MaxLength(50)]
        public string? Stipend { get; set; }

        [MaxLength(50)]
        public string? Bond { get; set; }

        [Required]
        public DateTime DateOfJoining { get; set; }

        // Eligibility
        public string? EligibleBatches { get; set; }    // Comma separated
        public string? EligibleCourses { get; set; }    // Comma separated

        // Selection Process
        [Required]
        public string SelectionProcess { get; set; } = string.Empty;

        [Required]
        public DateTime CampusDriveDate { get; set; }

        [MaxLength(200)]
        public string? RegistrationLink { get; set; }

        public string? AdditionalNotes { get; set; }

        // Contact Person
        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string ContactMobile { get; set; } = string.Empty;
    }
}
