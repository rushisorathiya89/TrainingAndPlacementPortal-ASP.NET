using System;
using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.Models
{
    public class JobPosting
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobPosition { get; set; }

        [Required]
        [MaxLength(50)]
        public string AnnualCTC { get; set; }

        [MaxLength(50)]
        public string InternshipDuration { get; set; }
        
        [MaxLength(50)]
        public string Stipend { get; set; }

        [MaxLength(50)]
        public string Bond { get; set; }

        public DateTime DateOfJoining { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobLocation { get; set; }

        public string SelectionProcess { get; set; }

        public DateTime CampusDriveDate { get; set; }
        
        [MaxLength(200)]
        public string RegistrationLink { get; set; }

        public string AdditionalNotes { get; set; }
        public string DocumentsPath { get; set; }

        // Comma separated values for simple storage
        public string EligibleBatches { get; set; } 
        public string EligibleCourses { get; set; }

        // Approval workflow
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, OnHold

        public double? MinCGPA { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for applications
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
