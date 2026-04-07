using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Data;

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
    }
}
