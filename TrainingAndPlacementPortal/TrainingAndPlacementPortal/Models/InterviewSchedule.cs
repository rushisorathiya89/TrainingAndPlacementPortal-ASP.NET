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
        public int RoundNumber { get; set; } = 1;

        [MaxLength(100)]
        public string RoundName { get; set; } = string.Empty;

        [Required]
        public DateTime InterviewDate { get; set; }

        [MaxLength(50)]
        public string Timing { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Venue { get; set; } = string.Empty; 

        [MaxLength(100)]
        public string WaitingArea { get; set; } = string.Empty;

        [MaxLength(20)]
        public string InterviewType { get; set; } = "Offline"; // Online, Offline

        [MaxLength(200)]
        public string LocationOrLink { get; set; } = string.Empty; // For Online link or Detailed location
        
        public string Instructions { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
