using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Where(s => s.User.Role == "Student")
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
                    Email = s.User.Email,
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
                    IsApproved = s.User.IsApproved,
                    s.RegisteredAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = students });
        }

        // PUT: api/admin/students/{id}/approve
        [HttpPut("students/{id}/approve")]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            student.IsApproved = true;
            student.User.IsApproved = true;
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
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            student.IsApproved = false;
            student.User.IsApproved = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"{student.FullName} has been rejected." });
        }

        // DELETE: api/admin/students/{id}
        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            var studentName = student.FullName;

            // Remove student record
            _context.Students.Remove(student);

            // Remove associated user record
            if (student.User != null)
            {
                _context.Users.Remove(student.User);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"{studentName} has been deleted successfully." });
        }

<<<<<<< HEAD
        // --- Placement History ---
=======
        // GET: api/admin/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalStudents = await _context.Students.Include(s => s.User).CountAsync(s => s.User.Role == "Student");
            var pendingApprovals = await _context.JobPostings.CountAsync(j => j.Status == "Pending");
            var activeJobDrives = await _context.JobPostings.CountAsync(j => j.Status == "Approved" && j.IsActive);
            var totalApplications = await _context.JobApplications.CountAsync();
            var shortlistedStudents = await _context.JobApplications.CountAsync(a => a.ApplicationStatus == "Shortlisted");
            var selectedStudents = await _context.JobApplications.CountAsync(a => a.ApplicationStatus == "Selected");

            return Ok(new 
            { 
                success = true, 
                data = new {
                    totalStudents,
                    pendingApprovals,
                    activeJobDrives,
                    totalApplications,
                    shortlistedStudents,
                    selectedStudents
                }
            });
        }
>>>>>>> 851760d24f17ea08d5a29a3df5f5dab26368342f

        // GET: api/admin/placement-history
        [HttpGet("placement-history")]
        public async Task<IActionResult> GetPlacementHistory()
        {
            var history = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Applications)
                .Where(j => j.Status == "Approved" || j.Status == "Completed" || j.Status == "Rejected")
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    CompanyName = j.Company.CompanyName,
                    JobRole = j.JobPosition,
                    Package = j.AnnualCTC,
                    OfferDate = j.CampusDriveDate, // Using Campus Drive Date as a proxy for offer processing start
                    JoiningDate = j.DateOfJoining,
                    SelectedStudents = j.Applications.Count(a => a.ApplicationStatus == "Selected"),
                    StatusLabel = j.Status // Can map to badges in UI
                })
                .ToListAsync();

            return Ok(new { success = true, data = history });
        }

        // GET: api/admin/interview-schedules/{jobId}
        [HttpGet("interview-schedules/{jobId}")]
        public async Task<IActionResult> GetInterviewSchedules(int jobId)
        {
            var schedules = await _context.InterviewSchedules
                .Where(s => s.JobPostingId == jobId)
                .OrderBy(s => s.RoundNumber)
                .ToListAsync();

            return Ok(new { success = true, data = schedules });
        }

        public class InterviewRoundDto
        {
            public int RoundNumber { get; set; }
            public string RoundName { get; set; }
            public string Venue { get; set; }
            public string WaitingArea { get; set; }
            public DateTime InterviewDate { get; set; }
            public string Timing { get; set; }
        }

        // POST: api/admin/interview-schedules/{jobId}
        [HttpPost("interview-schedules/{jobId}")]
        public async Task<IActionResult> SaveInterviewSchedules(int jobId, [FromBody] List<InterviewRoundDto> rounds)
        {
            var job = await _context.JobPostings.FindAsync(jobId);
            if (job == null) return NotFound(new { success = false, message = "Job not found." });

            // Clear existing rounds
            var existing = _context.InterviewSchedules.Where(s => s.JobPostingId == jobId);
            _context.InterviewSchedules.RemoveRange(existing);

            // Add new rounds
            foreach (var r in rounds)
            {
                _context.InterviewSchedules.Add(new InterviewSchedule
                {
                    JobPostingId = jobId,
                    RoundNumber = r.RoundNumber,
                    RoundName = r.RoundName,
                    Venue = r.Venue,
                    WaitingArea = r.WaitingArea,
                    InterviewDate = r.InterviewDate,
                    Timing = r.Timing,
                    InterviewType = "Offline", // Default
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Interview rounds updated successfully." });
        }

        public class AddPortalUserDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // POST: api/admin/add-portal-user
        [HttpPost("add-portal-user")]
        public async Task<IActionResult> AddPortalUser([FromBody] AddPortalUserDto dto)
        {
            var existing = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existing) return BadRequest(new { success = false, message = "Email already registered." });

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Admin",
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User added successfully!" });
        }
    }
    }
