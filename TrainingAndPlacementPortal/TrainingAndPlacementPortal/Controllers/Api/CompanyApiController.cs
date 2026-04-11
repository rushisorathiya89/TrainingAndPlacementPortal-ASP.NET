using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.DTOs;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/company")]
    public class CompanyApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyApiController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetCompanyId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return 0;
            
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
            return company?.Id ?? 0;
        }

        // ===== COMPANY: Get my own job postings =====
        // GET: api/company/my-jobs
        [HttpGet("my-jobs")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetMyJobs()
        {
            var companyId = await GetCompanyId();
            if (companyId == 0) return NotFound(new { success = false, message = "Company profile not found." });

            var postings = await _context.JobPostings
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.JobPosition,
                    j.AnnualCTC,
                    j.Status,
                    j.IsActive,
                    j.PostedAt,
                    ApplicantCount = _context.JobApplications.Count(a => a.JobPostingId == j.Id)
                })
                .ToListAsync();

            return Ok(new { success = true, data = postings });
        }

        // ===== PUBLIC: Recruiter submits JD =====
        // POST: api/company/submit-jd
        [HttpPost("submit-jd")]
        public async Task<IActionResult> SubmitJd([FromBody] SubmitJdDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                // Check if company with same email already exists
                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Email == dto.Email);

                Company company;

                if (existingCompany != null)
                {
                    // Use existing company
                    company = existingCompany;
                    // Update contact info if changed
                    company.ContactName = dto.ContactName;
                    company.ContactEmail = dto.ContactEmail;
                    company.ContactMobile = dto.ContactMobile;
                    company.Website = dto.Website ?? company.Website;
                    company.Description = dto.Description ?? company.Description;
                }
                else
                {
                    // Check if a user with this email already exists
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                    User user;
                    
                    if (existingUser == null)
                    {
                        // Create a new User account for the company
                        user = new User
                        {
                            Email = dto.Email,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Company@123"), // Default password
                            Role = "Company",
                            IsApproved = true, // Auto-approve for JD submission for now
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        user = existingUser;
                        // Ensure user has Company role if they are submitting a JD
                        if (user.Role == "Student") 
                        {
                             return BadRequest(new { success = false, message = "Email is already registered as a student." });
                        }
                        user.Role = "Company";
                    }

                    // Create new company linked to user
                    company = new Company
                    {
                        UserId = user.Id,
                        CompanyName = dto.CompanyName,
                        Email = dto.Email,
                        Website = dto.Website ?? "",
                        JobLocation = dto.JobLocation ?? "",
                        Description = dto.Description ?? "",
                        ContactName = dto.ContactName,
                        ContactEmail = dto.ContactEmail,
                        ContactMobile = dto.ContactMobile,
                        Status = "Pending",
                        RegisteredAt = DateTime.UtcNow
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync(); 
                }

                // Create job posting linked to company
                var jobPosting = new JobPosting
                {
                    CompanyId = company.Id,
                    JobPosition = dto.JobPosition,
                    AnnualCTC = dto.AnnualCTC,
                    InternshipDuration = dto.InternshipDuration ?? "",
                    Stipend = dto.Stipend ?? "",
                    Bond = dto.Bond ?? "",
                    DateOfJoining = dto.DateOfJoining,
                    EligibleBatches = dto.EligibleBatches ?? "",
                    EligibleCourses = dto.EligibleCourses ?? "",
                    SelectionProcess = dto.SelectionProcess,
                    CampusDriveDate = dto.CampusDriveDate,
                    RegistrationLink = dto.RegistrationLink ?? "",
                    AdditionalNotes = dto.AdditionalNotes ?? "",
                    DocumentsPath = "",
                    Status = "Pending",
                    IsActive = true,
                    PostedAt = DateTime.UtcNow
                };

                _context.JobPostings.Add(jobPosting);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "JD submitted successfully! Our T&P team will review it shortly."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Submission failed. Please try again.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ===== ADMIN: Get all job postings =====
        // GET: api/company/job-postings
        [HttpGet("job-postings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllJobPostings()
        {
            var postings = await _context.JobPostings
                .Include(j => j.Company)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.CompanyId,
                    CompanyName = j.Company.CompanyName,
                    CompanyEmail = j.Company.Email,
                    j.Company.Website,
                    j.Company.JobLocation,
                    j.Company.Description,
                    j.Company.ContactName,
                    j.Company.ContactEmail,
                    j.Company.ContactMobile,
                    j.JobPosition,
                    j.AnnualCTC,
                    j.InternshipDuration,
                    j.Stipend,
                    j.Bond,
                    j.DateOfJoining,
                    j.EligibleBatches,
                    j.EligibleCourses,
                    j.SelectionProcess,
                    j.CampusDriveDate,
                    j.RegistrationLink,
                    j.AdditionalNotes,
                    j.Status,
                    j.MinCGPA,
                    j.IsActive,
                    j.PostedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = postings });
        }

        // ===== ADMIN: Get jobs eligible for interview scheduling (Approved + Has Applications) =====
        // GET: api/company/admin/schedulable-jobs
        [HttpGet("admin/schedulable-jobs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSchedulableJobs()
        {
            var jobs = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.JobApplications)
                .Where(j => j.Status == "Approved" && j.JobApplications.Any())
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    CompanyName = j.Company.CompanyName,
                    JobRole = j.JobPosition,
                    Package = j.AnnualCTC,
                    ApplicationCount = j.JobApplications.Count
                })
                .ToListAsync();

            return Ok(new { success = true, data = jobs });
        }

        // ===== ADMIN: Get single job posting detail =====
        // GET: api/company/job-postings/{id}
        [HttpGet("job-postings/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetJobPosting(int id)
        {
            var j = await _context.JobPostings
                .Include(jp => jp.Company)
                .Where(jp => jp.Id == id)
                .Select(jp => new
                {
                    jp.Id,
                    jp.CompanyId,
                    CompanyName = jp.Company.CompanyName,
                    CompanyEmail = jp.Company.Email,
                    jp.Company.Website,
                    jp.Company.JobLocation,
                    jp.Company.Description,
                    jp.Company.ContactName,
                    jp.Company.ContactEmail,
                    jp.Company.ContactMobile,
                    jp.Company.AdminRemarks,
                    jp.JobPosition,
                    jp.AnnualCTC,
                    jp.InternshipDuration,
                    jp.Stipend,
                    jp.Bond,
                    jp.DateOfJoining,
                    jp.EligibleBatches,
                    jp.EligibleCourses,
                    jp.SelectionProcess,
                    jp.CampusDriveDate,
                    jp.RegistrationLink,
                    jp.AdditionalNotes,
                    jp.Status,
                    jp.MinCGPA,
                    jp.IsActive,
                    jp.PostedAt
                })
                .FirstOrDefaultAsync();

            if (j == null)
                return NotFound(new { success = false, message = "Job posting not found." });

            return Ok(new { success = true, data = j });
        }

        // ===== ADMIN: Update job posting status =====
        // PUT: api/company/job-postings/{id}/status
        [HttpPut("job-postings/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateJobPostingStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var jobPosting = await _context.JobPostings
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobPosting == null)
                return NotFound(new { success = false, message = "Job posting not found." });

            var validStatuses = new[] { "Pending", "Approved", "Rejected", "OnHold" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { success = false, message = "Invalid status. Use: Pending, Approved, Rejected, OnHold." });

            jobPosting.Status = dto.Status;

            // Sync company status
            if (dto.Status == "Approved")
                jobPosting.Company.Status = "Verified";
            else if (dto.Status == "Rejected")
                jobPosting.Company.Status = "Rejected";
            else
                jobPosting.Company.Status = "Pending";

            if (!string.IsNullOrEmpty(dto.Remarks))
                jobPosting.Company.AdminRemarks = dto.Remarks;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Job posting status updated to {dto.Status}." });
        }

        // ===== ADMIN: Update/Edit job posting details =====
        // PUT: api/company/job-postings/{id}
        [HttpPut("job-postings/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateJobPosting(int id, [FromBody] SubmitJdDto dto)
        {
            var jobPosting = await _context.JobPostings
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobPosting == null)
                return NotFound(new { success = false, message = "Job posting not found." });

            // Update company details
            jobPosting.Company.CompanyName = dto.CompanyName;
            jobPosting.Company.Email = dto.Email;
            jobPosting.Company.Website = dto.Website ?? "";
            jobPosting.Company.JobLocation = dto.JobLocation ?? "";
            jobPosting.Company.Description = dto.Description ?? "";
            jobPosting.Company.ContactName = dto.ContactName;
            jobPosting.Company.ContactEmail = dto.ContactEmail;
            jobPosting.Company.ContactMobile = dto.ContactMobile;

            // Update job posting details
            jobPosting.JobPosition = dto.JobPosition;
            jobPosting.AnnualCTC = dto.AnnualCTC;
            jobPosting.InternshipDuration = dto.InternshipDuration ?? "";
            jobPosting.Stipend = dto.Stipend ?? "";
            jobPosting.Bond = dto.Bond ?? "";
            jobPosting.DateOfJoining = dto.DateOfJoining;
            jobPosting.EligibleBatches = dto.EligibleBatches ?? "";
            jobPosting.EligibleCourses = dto.EligibleCourses ?? "";
            jobPosting.SelectionProcess = dto.SelectionProcess;
            jobPosting.CampusDriveDate = dto.CampusDriveDate;
            jobPosting.RegistrationLink = dto.RegistrationLink ?? "";
            jobPosting.AdditionalNotes = dto.AdditionalNotes ?? "";

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Job posting updated successfully." });
        }

        // ===== ADMIN: Delete job posting =====
        // DELETE: api/company/job-postings/{id}
        [HttpDelete("job-postings/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteJobPosting(int id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);

            if (jobPosting == null)
                return NotFound(new { success = false, message = "Job posting not found." });

            _context.JobPostings.Remove(jobPosting);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Job posting deleted successfully." });
        }

        // ===== STUDENT: Get approved jobs only =====
        // GET: api/company/approved-jobs
        [HttpGet("approved-jobs")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetApprovedJobs()
        {
            var jobs = await _context.JobPostings
                .Include(j => j.Company)
                .Where(j => j.Status == "Approved" && j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    CompanyName = j.Company.CompanyName,
                    j.JobPosition,
                    j.AnnualCTC,
                    j.Company.JobLocation,
                    j.MinCGPA,
                    j.CampusDriveDate,
                    j.PostedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = jobs });
        }

        // ===== STUDENT: Get single approved job detail =====
        // GET: api/company/approved-jobs/{id}
        [HttpGet("approved-jobs/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetApprovedJobDetail(int id)
        {
            var j = await _context.JobPostings
                .Include(jp => jp.Company)
                .Where(jp => jp.Id == id && jp.Status == "Approved" && jp.IsActive)
                .Select(jp => new
                {
                    jp.Id,
                    CompanyName = jp.Company.CompanyName,
                    jp.Company.Website,
                    jp.Company.JobLocation,
                    jp.Company.Description,
                    jp.JobPosition,
                    jp.AnnualCTC,
                    jp.InternshipDuration,
                    jp.Stipend,
                    jp.Bond,
                    jp.DateOfJoining,
                    jp.EligibleBatches,
                    jp.EligibleCourses,
                    jp.SelectionProcess,
                    jp.CampusDriveDate,
                    jp.RegistrationLink,
                    jp.AdditionalNotes,
                    jp.MinCGPA,
                    jp.PostedAt
                })
                .FirstOrDefaultAsync();

            if (j == null)
                return NotFound(new { success = false, message = "Job not found." });

            return Ok(new { success = true, data = j });
        }
    }

    // Small DTO for status updates
    public class UpdateStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
