using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public class StudentProfileApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentProfileApiController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // GET: api/student/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student profile not found." });
            }

            // Fetch recent applications
            var recentApplications = await _context.JobApplications
                .Include(a => a.JobPosting)
                .ThenInclude(jp => jp.Company)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AppliedAt)
                .Take(3)
                .Select(a => new
                {
                    JobPosition = a.JobPosting.JobPosition,
                    CompanyName = a.JobPosting.Company.CompanyName,
                    Status = a.ApplicationStatus,
                    AppliedAt = a.AppliedAt,
                    JobType = "Full-time" // Mocked as JobPosting doesn't have it yet, or add if exists
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    student.FullName,
                    student.EnrollmentNumber,
                    student.Branch,
                    student.Semester,
                    student.MobileNumber,
                    Email = student.User?.Email,
                    student.PersonalEmail,
                    student.ParentName,
                    student.ParentMobile,
                    student.CGPA,
                    student.DateOfBirth,
                    AcademicYear = $"{student.RegisteredAt.Year}-{student.RegisteredAt.Year + 1}", // Simple calculation
                    RecentApplications = recentApplications
                }
            });
        }

        // PUT: api/student/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student profile not found." });
            }

            // Update allowed fields
            student.FullName = dto.FullName ?? student.FullName;
            student.MobileNumber = dto.MobileNumber ?? student.MobileNumber;
            student.ParentName = dto.ParentName ?? student.ParentName;
            student.ParentMobile = dto.ParentMobile ?? student.ParentMobile;
            student.PersonalEmail = dto.PersonalEmail ?? student.PersonalEmail;
            student.Semester = dto.Semester ?? student.Semester;
            student.DateOfBirth = dto.DateOfBirth ?? student.DateOfBirth;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Profile updated successfully." });
        }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string? ParentName { get; set; }
        public string? ParentMobile { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Semester { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
