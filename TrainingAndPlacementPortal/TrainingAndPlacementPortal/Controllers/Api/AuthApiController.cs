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
    }
}
