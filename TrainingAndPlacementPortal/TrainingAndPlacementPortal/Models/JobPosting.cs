using System;
using System.Collections.Generic;
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

        public string SelectionProcess { get; set; }

        public DateTime CampusDriveDate { get; set; }
        
        [MaxLength(200)]
        public string RegistrationLink { get; set; }

        public string AdditionalNotes { get; set; }
        public string DocumentsPath { get; set; }

        // Comma separated values for simple storage, or can use relational tables
        public string EligibleBatches { get; set; } 
        public string EligibleCourses { get; set; }

        // Added properties to match usage across the codebase
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // e.g. Pending, Approved, Rejected

        // Minimum CGPA required for the posting (0.0 means no minimum)
        public double MinCGPA { get; set; } = 0.0;
        
        public bool IsActive { get; set; } = true;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for applications
        public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }
}
