namespace TrainingAndPlacementPortal.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public string Message { get; set; }
    }
}
