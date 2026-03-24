using System;
using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.Models
{
    public class InterviewSchedule
    {
        [Key]
        public int Id { get; set; }

        public int JobPostingId { get; set; }
        public JobPosting JobPosting { get; set; }

        [Required]
        public DateTime InterviewDate { get; set; }

        [MaxLength(20)]
        public string InterviewType { get; set; } // Online, Offline

        [MaxLength(200)]
        public string LocationOrLink { get; set; }
        
        public string Instructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
