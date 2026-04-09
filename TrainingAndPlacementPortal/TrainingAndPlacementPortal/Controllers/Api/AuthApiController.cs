using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.DTOs;
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

        public AuthApiController(AppDbContext context, JwtTokenService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Validation failed.", errors });
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "An account with this email already exists." });
            }

            // Check if enrollment number already exists
            var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.EnrollmentNumber == dto.EnrollmentNumber);
            if (existingStudent != null)
            {
                return BadRequest(new { success = false, message = "A student with this enrollment number already exists." });
            }

            try
            {
                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // Create User
                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    Role = "Student",
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);

                // Create Student profile linked to user (will be saved together)
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
                    IsApproved = false,
                    RegisteredAt = DateTime.UtcNow
                };

                _context.Students.Add(student);

                // Save both User and Student in a single transaction
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
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Please fill all required fields." });
            }

            // Find user by email and role
            var user = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Role == dto.Role);

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }

            // Check if student is approved
            if (user.Role == "Student" && !user.IsApproved)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "Your account is pending admin approval. Please contact the T&P Cell for more information."
                });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Get full name
            var fullName = user.Role == "Admin" ? "Administrator" : user.Student?.FullName ?? "User";

            return Ok(new AuthResponseDto
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

        // ===== NEW ENDPOINTS =====

        // GET: api/auth/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var user = await _context.Users.Include(u => u.Student).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // We use the Student profile to shadow Admin Profile details as well
            return Ok(new
            {
                success = true,
                data = new
                {
                    FullName = user.Student?.FullName ?? "Administrator",
                    Email = user.Email,
                    MobileNumber = user.Student?.MobileNumber ?? "+91 00000 00000",
                    Role = user.Student?.Branch ?? user.Role,
                    EmpId = user.Student?.EnrollmentNumber ?? "ADMIN001",
                    Department = "Training & Placement Cell",
                    Location = "Office No. 27"
                }
            });
        }

        public class UpdateProfileDto
        {
            public string FullName { get; set; }
            public string MobileNumber { get; set; }
            public string Role { get; set; }
            public string EmpId { get; set; }
            public string Department { get; set; }
            public string Location { get; set; }
        }

        // PUT: api/auth/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var user = await _context.Users.Include(u => u.Student).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (user.Student == null)
            {
                user.Student = new Student { UserId = user.Id, RegisteredAt = DateTime.UtcNow };
                _context.Students.Add(user.Student);
            }

            user.Student.FullName = dto.FullName;
            user.Student.MobileNumber = dto.MobileNumber;
            user.Student.Branch = dto.Role; // Storing role here
            user.Student.EnrollmentNumber = dto.EmpId; // Storing EmpId here
            // Dept / Location omitted for brevity as they just shadow.

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Profile updated successfully." });
        }

        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Current password is incorrect." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully." });
        }

        public class ForgotPasswordDto
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }

        // POST: api/auth/forgot-password-reset
        [HttpPost("forgot-password-reset")]
        public async Task<IActionResult> ForgotPasswordReset([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                // Return success to prevent email enumeration, but for our demo we can return error
                return BadRequest(new { success = false, message = "Email not found." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password has been reset successfully." });
        }
    }
}
