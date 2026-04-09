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
        // ===== STUDENT: Get dashboard stats =====
        // GET: api/applications/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentDashboardStats()
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return BadRequest(new { success = false, message = "Student profile not found." });
            }

            var totalApplications = await _context.JobApplications
                .CountAsync(a => a.StudentId == student.Id);

            var shortlisting = await _context.JobApplications
                .CountAsync(a => a.StudentId == student.Id && (a.ApplicationStatus == "Shortlisted" || a.ApplicationStatus == "Interview"));

            var selections = await _context.JobApplications
                .CountAsync(a => a.StudentId == student.Id && (a.ApplicationStatus == "Placed" || a.ApplicationStatus == "Hired" || a.ApplicationStatus == "Selected"));

            // Upcoming interviews count from InterviewSchedules
            var upcomingInterviews = await _context.InterviewSchedules
                .CountAsync(i => i.InterviewDate >= DateTime.UtcNow && i.Status == "Pending");

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalApplications,
                    shortlisting,
                    selections,
                    upcomingInterviews
                }
            });
        }

        // ===== ADMIN: Get all companies that have applications =====
        // GET: api/applications/admin/applied-companies
        [HttpGet("admin/applied-companies")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppliedCompanies()
        {
            var companies = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.JobApplications)
                .Where(j => j.JobApplications.Any() && j.Status == "Approved")
                .Select(j => new
                {
                    JobPostingId = j.Id,
                    CompanyName = j.Company.CompanyName,
                    CompanyId = j.Company.Id,
                    JobRole = j.JobPosition,
                    ApplicationCount = j.JobApplications.Count
                })
                .ToListAsync();

            return Ok(new { success = true, data = companies });
        }

        // ===== ADMIN: Get students who applied for a specific job/company =====
        // GET: api/applications/admin/job/{jobId}/students
        [HttpGet("admin/job/{jobId}/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppliedStudents(int jobId)
        {
            var students = await _context.JobApplications
                .Include(a => a.Student)
                .Where(a => a.JobPostingId == jobId)
                .Select(a => new
                {
                    ApplicationId = a.Id,
                    a.StudentId,
                    StudentName = a.Student.FullName,
                    a.Student.EnrollmentNumber,
                    a.Student.Branch,
                    a.Student.Semester,
                    a.Student.CGPA,
                    a.ApplicationStatus,
                    a.AppliedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = students });
        }

        // ===== ADMIN: Update student application status =====
        // PUT: api/applications/admin/{applicationId}/status
        [HttpPut("admin/{applicationId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromBody] UpdateApplicationStatusDto dto)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound(new { success = false, message = "Application not found." });
            }

            var validStatuses = new[] { "Applied", "Shortlisted", "Selected", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
            {
                return BadRequest(new { success = false, message = "Invalid status." });
            }

            application.ApplicationStatus = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Application status updated successfully." });
        }
    }

    public class UpdateApplicationStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
