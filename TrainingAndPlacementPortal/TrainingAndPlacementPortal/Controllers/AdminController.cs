using Microsoft.AspNetCore.Mvc;

namespace TrainingAndPlacementPortal.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet("/AdminDashboard")]
        [HttpGet("/Admin/Dashboard")]
        public IActionResult Dashboard() => View();

        [HttpGet("/AdminProfile")]
        [HttpGet("/Admin/Profile")]
        public IActionResult Profile() => View();

        [HttpGet("/AdminCompanyManagement")]
        [HttpGet("/Admin/CompanyManagement")]
        public IActionResult CompanyManagement() => View();

        [HttpGet("/AdminJdDetail")]
        [HttpGet("/Admin/JdDetail")]
        public IActionResult JdDetail() => View();

        [HttpGet("/AdminStudentsManagement")]
        [HttpGet("/Admin/StudentsManagement")]
        public IActionResult StudentsManagement() => View();

        [HttpGet("/Admin/AppliedStudents")]
        public IActionResult AppliedStudents() => View();

        [HttpGet("/AdminInterviewSchedule")]
        [HttpGet("/Admin/InterviewSchedule")]
        public IActionResult InterviewSchedule() => View();

        [HttpGet("/AdminInterviewScheduleForm")]
        [HttpGet("/Admin/InterviewScheduleForm")]
        public IActionResult InterviewScheduleForm() => View();

        [HttpGet("/AdminPlacementHistory")]
        [HttpGet("/Admin/PlacementHistory")]
        public IActionResult PlacementHistory() => View();

        [HttpGet("/AdminChangePassword")]
        [HttpGet("/Admin/ChangePassword")]
        public IActionResult ChangePassword() => View();

        [HttpGet("/AdminForgotPassword")]
        [HttpGet("/Admin/ForgotPassword")]
        public IActionResult ForgotPassword() => View();
    }
}
