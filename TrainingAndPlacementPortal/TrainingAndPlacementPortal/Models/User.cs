using System;
using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty; // "Student", "Admin", "Company"

        public bool IsApproved { get; set; } = false; // For students, to indicate if approved by admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Student? Student { get; set; }
    }
}
