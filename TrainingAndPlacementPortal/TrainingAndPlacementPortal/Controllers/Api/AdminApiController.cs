using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;
using System.Collections.Generic;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminApiController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalStudents = await _context.Students.CountAsync();
                var pendingApprovals = await _context.Users.CountAsync(u => u.Role == "Student" && !u.IsApproved);
                var activeJobDrives = await _context.JobPostings.CountAsync(j => j.Status == "Approved" && j.IsActive);
                var totalApplications = await _context.JobApplications.CountAsync();
                
                // For demonstration, these might rely on specific application statuses
                var shortlisted = await _context.JobApplications.CountAsync(a => a.ApplicationStatus == "Shortlisted" || a.ApplicationStatus == "Interview");
                var placed = await _context.JobApplications.CountAsync(a => a.ApplicationStatus == "Placed" || a.ApplicationStatus == "Hired");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalStudents,
                        pendingApprovals,
                        activeJobDrives,
                        totalApplications,
                        shortlisted,
                        placed
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch stats.", error = ex.Message });
            }
        }

        // GET: api/admin/students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .OrderByDescending(s => s.RegisteredAt)
                .Select(s => new
                {
                    s.Id,
                    s.UserId,
                    s.FullName,
                    s.EnrollmentNumber,
                    s.Branch,
                    s.Semester,
                    s.CGPA,
                    Email = s.User!.Email,
                    s.MobileNumber,
                    s.PersonalEmail,
                    s.TenthPercentage,
                    s.TwelfthPercentage,
                    s.ParentName,
                    s.ParentEmail,
                    s.ParentMobile,
                    s.CurrentAddress,
                    s.PermanentAddress,
                    s.City,
                    s.State,
                    s.Pincode,
                    s.ConsentFormPath,
                    s.PaymentProofPath,
                    s.PaymentStatus,
                    s.RazorpayPaymentId,
                    s.PaymentAmount,
                    IsApproved = s.User.IsApproved,
                    s.RegisteredAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = students });
        }

        // GET: api/admin/students/{id}/document/{fileType}
        [HttpGet("students/{id}/document/{fileType}")]
        public async Task<IActionResult> DownloadDocument(int id, string fileType)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { success = false, message = "Student not found." });

            string? relativePath = fileType.ToLower() switch
            {
                "consent" => student.ConsentFormPath,
                "payment" => student.PaymentProofPath,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(relativePath))
                return NotFound(new { success = false, message = "Document not found." });

            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { success = false, message = "File not found on server." });

            var contentType = Path.GetExtension(fullPath).ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            var fileName = $"{student.EnrollmentNumber}_{fileType}{Path.GetExtension(fullPath)}";
            return PhysicalFile(fullPath, contentType, fileName);
        }

        // PUT: api/admin/students/{id}/approve
        [HttpPut("students/{id}/approve")]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound(new { success = false, message = "Student not found." });

            student.IsApproved = true;
            student.User!.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"{student.FullName} has been approved successfully." });
        }

        // PUT: api/admin/students/{id}/reject
        [HttpPut("students/{id}/reject")]
        public async Task<IActionResult> RejectStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound(new { success = false, message = "Student not found." });

            student.IsApproved = false;
            student.User!.IsApproved = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"{student.FullName} has been rejected." });
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound(new { success = false, message = "Student not found." });

            var studentName = student.FullName;

            // Delete uploaded files
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "students", student.EnrollmentNumber);
            if (Directory.Exists(uploadDir))
            {
                Directory.Delete(uploadDir, true);
            }

            // Remove payment records
            var payments = await _context.Payments.Where(p => p.StudentId == id).ToListAsync();
            _context.Payments.RemoveRange(payments);

            _context.Students.Remove(student);
            if (student.User != null)
                _context.Users.Remove(student.User);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"{studentName} has been deleted successfully." });
        }

        // --- Interview Scheduling ---

        // GET: api/admin/interview-schedules
        [HttpGet("interview-schedules")]
        public async Task<IActionResult> GetInterviewSchedules()
        {
            var schedules = await _context.InterviewSchedules
                .Include(i => i.JobPosting)
                .ThenInclude(j => j.Company)
                .OrderByDescending(i => i.InterviewDate)
                .Select(i => new
                {
                    i.Id,
                    i.JobPostingId,
                    CompanyName = i.JobPosting.Company.CompanyName,
                    JobRole = i.JobPosting.JobPosition,
                    i.RoundNumber,
                    i.RoundName,
                    i.InterviewDate,
                    i.InterviewType,
                    i.LocationOrLink,
                    i.WaitingArea,
                    i.Instructions,
                    i.Status,
                    AnnualCTC = i.JobPosting.AnnualCTC
                })
                .ToListAsync();

            return Ok(new { success = true, data = schedules });
        }

        // POST: api/admin/interview-schedules
        [HttpPost("interview-schedules")]
        public async Task<IActionResult> CreateInterviewSchedules([FromBody] List<InterviewSchedule> schedules)
        {
            if (schedules == null || schedules.Count == 0)
                return BadRequest(new { success = false, message = "No schedules provided." });

            foreach (var s in schedules)
            {
                s.CreatedAt = DateTime.UtcNow;
                _context.InterviewSchedules.Add(s);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{schedules.Count} rounds scheduled successfully." });
        }

        // --- Placement History ---

        // GET: api/admin/placement-history
        [HttpGet("placement-history")]
        public async Task<IActionResult> GetPlacementHistory()
        {
            var history = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.JobApplications)
                .Where(j => j.Status == "Approved")
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    CompanyName = j.Company.CompanyName,
                    JobRole = j.JobPosition,
                    Package = j.AnnualCTC,
                    SelectedCount = j.JobApplications.Count(a => a.ApplicationStatus == "Placed" || a.ApplicationStatus == "Hired"),
                    j.CampusDriveDate,
                    j.DateOfJoining,
                    Status = j.IsActive ? "In Progress" : "Completed"
                })
                .ToListAsync();

            return Ok(new { success = true, data = history });
        }
    }
}
