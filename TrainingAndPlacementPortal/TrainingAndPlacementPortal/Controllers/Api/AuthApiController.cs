using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;
using TrainingAndPlacementPortal.Services;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenService _jwtService;
        private readonly RazorpayService _razorpayService;
        private readonly IWebHostEnvironment _env;

        public AuthApiController(AppDbContext context, JwtTokenService jwtService, RazorpayService razorpayService, IWebHostEnvironment env)
        {
            _context = context;
            _jwtService = jwtService;
            _razorpayService = razorpayService;
            _env = env;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterFormData dto)
        {
            // --- Basic validation ---
            if (string.IsNullOrWhiteSpace(dto.EnrollmentNumber) ||
                string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Branch) ||
                string.IsNullOrWhiteSpace(dto.Semester) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.MobileNumber))
            {
                return BadRequest(new { success = false, message = "Please fill all required fields." });
            }

            if (dto.Password.Length < 6)
                return BadRequest(new { success = false, message = "Password must be at least 6 characters." });

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { success = false, message = "Passwords do not match." });

            // --- Check duplicates ---
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { success = false, message = "An account with this email already exists." });

            if (await _context.Students.AnyAsync(s => s.EnrollmentNumber == dto.EnrollmentNumber))
                return BadRequest(new { success = false, message = "A student with this enrollment number already exists." });

            // --- Verify Razorpay payment (if payment was made) ---
            string paymentStatus = "Pending";
            string? razorpayOrderId = dto.RazorpayOrderId;
            string? razorpayPaymentId = dto.RazorpayPaymentId;

            if (!string.IsNullOrWhiteSpace(dto.RazorpayOrderId) &&
                !string.IsNullOrWhiteSpace(dto.RazorpayPaymentId) &&
                !string.IsNullOrWhiteSpace(dto.RazorpaySignature))
            {
                bool isValid = _razorpayService.VerifyPaymentSignature(
                    dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature);

                if (!isValid)
                    return BadRequest(new { success = false, message = "Payment verification failed. Please try again." });

                paymentStatus = "Paid";
            }

            try
            {
                // --- Handle file uploads ---
                string? consentFormPath = null;
                string? paymentProofPath = null;

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "students", dto.EnrollmentNumber);
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                if (dto.ConsentForm != null && dto.ConsentForm.Length > 0)
                {
                    var ext = Path.GetExtension(dto.ConsentForm.FileName).ToLower();
                    if (ext != ".pdf")
                        return BadRequest(new { success = false, message = "Consent form must be a PDF file." });
                    if (dto.ConsentForm.Length > 5 * 1024 * 1024) // 5MB limit
                        return BadRequest(new { success = false, message = "Consent form must be less than 5MB." });

                    var fileName = $"consent_form{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await dto.ConsentForm.CopyToAsync(stream);
                    consentFormPath = $"/uploads/students/{dto.EnrollmentNumber}/{fileName}";
                }

                if (dto.PaymentProof != null && dto.PaymentProof.Length > 0)
                {
                    var ext = Path.GetExtension(dto.PaymentProof.FileName).ToLower();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExts.Contains(ext))
                        return BadRequest(new { success = false, message = "Payment proof must be an image (JPG, PNG, WEBP)." });
                    if (dto.PaymentProof.Length > 5 * 1024 * 1024)
                        return BadRequest(new { success = false, message = "Payment proof must be less than 5MB." });

                    var fileName = $"payment_proof{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await dto.PaymentProof.CopyToAsync(stream);
                    paymentProofPath = $"/uploads/students/{dto.EnrollmentNumber}/{fileName}";
                }

                // --- Create User ---
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    Role = "Student",
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);

                // --- Create Student ---
                var student = new Student
                {
                    User = user,
                    EnrollmentNumber = dto.EnrollmentNumber,
                    FullName = dto.FullName,
                    Branch = dto.Branch,
                    Semester = dto.Semester,
                    PersonalEmail = dto.PersonalEmail ?? "",
                    MobileNumber = dto.MobileNumber,
                    TenthPercentage = dto.TenthPercentage,
                    TwelfthPercentage = dto.TwelfthPercentage,
                    CGPA = dto.CGPA,
                    ParentName = dto.ParentName ?? "",
                    ParentEmail = dto.ParentEmail ?? "",
                    ParentMobile = dto.ParentMobile ?? "",
                    CurrentAddress = dto.CurrentAddress ?? "",
                    PermanentAddress = dto.PermanentAddress ?? "",
                    City = dto.City ?? "",
                    State = dto.State ?? "",
                    Pincode = dto.Pincode ?? "",
                    ConsentFormPath = consentFormPath,
                    PaymentProofPath = paymentProofPath,
                    RazorpayOrderId = razorpayOrderId,
                    RazorpayPaymentId = razorpayPaymentId,
                    PaymentStatus = paymentStatus,
                    PaymentAmount = _razorpayService.PaymentAmount,
                    IsApproved = false,
                    RegisteredAt = DateTime.UtcNow
                };
                _context.Students.Add(student);

                // --- Create Payment record (if paid) ---
                if (paymentStatus == "Paid")
                {
                    var payment = new Payment
                    {
                        Student = student,
                        RazorpayOrderId = dto.RazorpayOrderId!,
                        RazorpayPaymentId = dto.RazorpayPaymentId,
                        RazorpaySignature = dto.RazorpaySignature,
                        Amount = _razorpayService.PaymentAmount,
                        Currency = _razorpayService.Currency,
                        Status = "Paid",
                        CreatedAt = DateTime.UtcNow,
                        PaidAt = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Registration successful! Please wait for admin approval before you can login."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Registration failed. Please try again.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Please fill all required fields." });

            var user = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == dto.Role);

            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid email or password." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { success = false, message = "Invalid email or password." });

            if (user.Role == "Student" && !user.IsApproved)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "Your account is pending admin approval. Please contact the T&P Cell for more information."
                });
            }

            var token = _jwtService.GenerateToken(user);
            var fullName = user.Role == "Admin" ? "Administrator" : user.Student?.FullName ?? "User";

            return Ok(new DTOs.AuthResponseDto
            {
                Success = true,
                Token = token,
                Role = user.Role,
                FullName = fullName,
                Email = user.Email,
                IsApproved = user.IsApproved,
                Message = "Login successful!"
            });
        }
    }

    /// <summary>
    /// Form data model for multipart registration (supports file uploads).
    /// </summary>
    public class RegisterFormData
    {
        public string EnrollmentNumber { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Branch { get; set; } = "";
        public string Semester { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PersonalEmail { get; set; }
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string MobileNumber { get; set; } = "";
        public double TenthPercentage { get; set; }
        public double TwelfthPercentage { get; set; }
        public double CGPA { get; set; }
        public string? ParentName { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentMobile { get; set; }
        public string? CurrentAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }

        // File uploads
        public IFormFile? ConsentForm { get; set; }
        public IFormFile? PaymentProof { get; set; }

        // Razorpay payment fields
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
    }
}
