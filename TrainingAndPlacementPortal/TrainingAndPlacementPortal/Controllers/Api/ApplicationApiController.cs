using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/applications")]
    public class ApplicationApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApplicationApiController(AppDbContext context)
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

        // ===== STUDENT: Apply for a job =====
        // POST: api/applications/apply/{jobId}
        [HttpPost("apply/{jobId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ApplyForJob(int jobId)
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return BadRequest(new { success = false, message = "Student profile not found." });
            }

            // Verify job exists and is approved and active
            var job = await _context.JobPostings
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Approved" && j.IsActive);

            if (job == null)
            {
                return NotFound(new { success = false, message = "Job not found or not available for application." });
            }

            // Check if already applied
            var existingApp = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.JobPostingId == jobId);

            if (existingApp != null)
            {
                return BadRequest(new { success = false, message = "You have already applied for this job." });
            }

            // Create application
            var application = new JobApplication
            {
                StudentId = student.Id,
                JobPostingId = jobId,
                ApplicationStatus = "Applied",
                AppliedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Application submitted successfully!" });
        }

        // ===== STUDENT: Get all my applications =====
        // GET: api/applications/my-applications
        [HttpGet("my-applications")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return BadRequest(new { success = false, message = "Student profile not found." });
            }

            var applications = await _context.JobApplications
                .Include(a => a.JobPosting)
                .ThenInclude(jp => jp.Company)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    a.Id,
                    a.JobPostingId,
                    CompanyName = a.JobPosting.Company.CompanyName,
                    JobRole = a.JobPosting.JobPosition,
                    Package = a.JobPosting.AnnualCTC,
                    a.ApplicationStatus,
                    a.AppliedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = applications });
        }

        // ===== STUDENT: Check specific application status =====
        // GET: api/applications/check/{jobId}
        [HttpGet("check/{jobId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckApplicationStatus(int jobId)
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return Ok(new { success = true, hasApplied = false });
            }

            var hasApplied = await _context.JobApplications
                .AnyAsync(a => a.StudentId == student.Id && a.JobPostingId == jobId);

            return Ok(new { success = true, hasApplied });
        }

        private async Task<int> GetCompanyId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return 0;
            
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
            return company?.Id ?? 0;
        }

        // ===== ADMIN/COMPANY: Get all students who applied for a specific job =====
        // GET: api/applications/job/{jobId}/students
        [HttpGet("job/{jobId}/students")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> GetStudentsForJob(int jobId)
        {
            // Security check for Company: only allow their own jobs
            if (User.IsInRole("Company"))
            {
                var companyId = await GetCompanyId();
                var ownsJob = await _context.JobPostings.AnyAsync(j => j.Id == jobId && j.CompanyId == companyId);
                if (!ownsJob) return Forbid();
            }

            var applications = await _context.JobApplications
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Where(a => a.JobPostingId == jobId)
                .Select(a => new
                {
                    a.Id,
                    StudentId = a.Student.Id,
                    a.Student.FullName,
                    a.Student.EnrollmentNumber,
                    a.Student.Branch,
                    a.Student.Semester,
                    a.Student.CGPA,
                    a.Student.MobileNumber,
                    Email = a.Student.User.Email,
                    a.ApplicationStatus,
                    a.AppliedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = applications });
        }

        // ===== ADMIN/COMPANY: Update student application status =====
        // PUT: api/applications/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] string status)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(new { success = false, message = "Application not found." });
            }

            // Security check for Company: only allow their own applicants
            if (User.IsInRole("Company"))
            {
                var companyId = await GetCompanyId();
                if (application.JobPosting.CompanyId != companyId) return Forbid();
            }

            application.ApplicationStatus = status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Status updated to {status}." });
        }
    }
}
