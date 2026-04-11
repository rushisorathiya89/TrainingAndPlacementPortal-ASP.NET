using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/admin/interview-schedules")]
    public class InterviewApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InterviewApiController(AppDbContext context)
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

        // ===== ADMIN: Save multi-round schedule =====
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveSchedule([FromBody] List<SaveInterviewScheduleDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest(new { success = false, message = "No schedules provided." });

            try
            {
                var jobPostingId = dtos[0].JobPostingId;

                // Remove existing rounds for this job posting to allow full update
                var existing = _context.InterviewSchedules.Where(s => s.JobPostingId == jobPostingId);
                _context.InterviewSchedules.RemoveRange(existing);

                foreach (var dto in dtos)
                {
                    var s = new InterviewSchedule
                    {
                        JobPostingId = dto.JobPostingId,
                        RoundNumber = dto.RoundNumber,
                        RoundName = dto.RoundName,
                        InterviewDate = dto.InterviewDate,
                        InterviewType = dto.InterviewType,
                        LocationOrLink = dto.LocationOrLink,
                        WaitingArea = dto.WaitingArea,
                        Instructions = dto.Instructions,
                        Status = dto.Status ?? "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.InterviewSchedules.Add(s);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Interview schedules saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to save schedules. Please check if the job drive exists and all data is valid.",
                    error = ex.InnerException?.Message ?? ex.Message 
                });
            }
        }

        // ===== ADMIN: Get all schedules =====
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSchedules()
        {
            var schedules = await _context.InterviewSchedules
                .Include(s => s.JobPosting)
                .ThenInclude(jp => jp.Company)
                .OrderBy(s => s.JobPostingId)
                .ThenBy(s => s.RoundNumber)
                .Select(s => new {
                    s.Id,
                    s.JobPostingId,
                    CompanyName = s.JobPosting.Company.CompanyName,
                    JobRole = s.JobPosting.JobPosition,
                    AnnualCTC = s.JobPosting.AnnualCTC,
                    s.RoundNumber,
                    s.RoundName,
                    s.InterviewDate,
                    s.InterviewType,
                    s.LocationOrLink,
                    s.Status
                })
                .ToListAsync();

            return Ok(new { success = true, data = schedules });
        }

        // ===== ADMIN: Update single round status =====
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var schedule = await _context.InterviewSchedules.FindAsync(id);
            if (schedule == null) return NotFound(new { success = false, message = "Schedule not found." });

            schedule.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Status updated successfully." });
        }

        // ===== STUDENT: Get my schedules =====
        [HttpGet("/api/student/interview-schedules")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentSchedules()
        {
            var userId = GetUserId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return Unauthorized(new { success = false, message = "Student profile not found." });

            // Only show schedules for jobs the student applied for and was NOT rejected
            var applications = await _context.JobApplications
                .Where(a => a.StudentId == student.Id && a.ApplicationStatus != "Rejected")
                .Select(a => new { a.JobPostingId, a.ApplicationStatus })
                .ToListAsync();

            var appliedJobIds = applications.Select(a => a.JobPostingId).ToList();

            var allSchedules = await _context.InterviewSchedules
                .Include(s => s.JobPosting)
                .ThenInclude(jp => jp.Company)
                .Where(s => appliedJobIds.Contains(s.JobPostingId))
                .OrderBy(s => s.JobPostingId)
                .ThenBy(s => s.RoundNumber)
                .ToListAsync();

            // Filter rounds based on eligibility
            var eligibleSchedules = allSchedules.Where(s => {
                var app = applications.FirstOrDefault(a => a.JobPostingId == s.JobPostingId);
                if (app == null) return false;

                // Round 1 is visible to all eligible (non-rejected) applicants
                if (s.RoundNumber == 1) return true;

                // Round 2+ visible only if Shortlisted or Selected
                return app.ApplicationStatus == "Shortlisted" || app.ApplicationStatus == "Selected";
            }).Select(s => new {
                s.Id,
                CompanyName = s.JobPosting.Company.CompanyName,
                JobRole = s.JobPosting.JobPosition,
                AnnualCTC = s.JobPosting.AnnualCTC,
                s.RoundNumber,
                s.RoundName,
                s.InterviewDate,
                s.InterviewType,
                s.LocationOrLink,
                s.WaitingArea,
                s.Instructions,
                s.Status
            }).ToList();

            return Ok(new { success = true, data = eligibleSchedules });
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class SaveInterviewScheduleDto
    {
        public int JobPostingId { get; set; }
        public int RoundNumber { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public DateTime InterviewDate { get; set; }
        public string? InterviewType { get; set; }
        public string? LocationOrLink { get; set; }
        public string? WaitingArea { get; set; }
        public string? Instructions { get; set; }
        public string? Status { get; set; }
    }
}
