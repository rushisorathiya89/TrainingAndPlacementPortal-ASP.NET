using System;
using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int JobPostingId { get; set; }
        public JobPosting JobPosting { get; set; }

        [MaxLength(20)]
        public string ApplicationStatus { get; set; } = "Applied"; // Applied, Shortlisted, Selected, Rejected

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
