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
        public int RoundNumber { get; set; }

        [Required, MaxLength(100)]
        public string RoundName { get; set; } // e.g., Aptitude, Technical, HR

        [Required]
        public DateTime InterviewDate { get; set; }

        [MaxLength(50)]
        public string InterviewType { get; set; } // Online, Offline

        [MaxLength(255)]
        public string LocationOrLink { get; set; }
        
        public string WaitingArea { get; set; }
        public string Instructions { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Complete, Canceled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
