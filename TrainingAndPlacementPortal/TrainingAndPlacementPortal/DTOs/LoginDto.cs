using System.ComponentModel.DataAnnotations;

namespace TrainingAndPlacementPortal.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }  // "Student" or "Admin"
    }
}
