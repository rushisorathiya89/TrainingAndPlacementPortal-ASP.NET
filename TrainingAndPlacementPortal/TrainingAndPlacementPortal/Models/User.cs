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
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // "Student", "Admin", "Company"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
