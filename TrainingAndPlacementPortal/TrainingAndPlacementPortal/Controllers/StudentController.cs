using Microsoft.AspNetCore.Mvc;

namespace TrainingAndPlacementPortal.Controllers
{
    public class StudentController : Controller
    {
        [HttpGet("/StudDashboard")]
        [HttpGet("/Student/Dashboard")]
        public IActionResult Dashboard() => View();

        [HttpGet("/StudProfile")]
        [HttpGet("/Student/Profile")]
        public IActionResult Profile() => View();

        [HttpGet("/StudJobs")]
        [HttpGet("/Student/Jobs")]
        public IActionResult Jobs() => View();

        [HttpGet("/StudJobDetails")]
        [HttpGet("/Student/JobDetails")]
        public IActionResult JobDetails() => View();

        [HttpGet("/StudApplications")]
        [HttpGet("/Student/Applications")]
        public IActionResult Applications() => View();

        [HttpGet("/StudInterviewSchedule")]
        [HttpGet("/Student/InterviewSchedule")]
        public IActionResult InterviewSchedule() => View();

        [HttpGet("/StudPlacementHistory")]
        [HttpGet("/Student/PlacementHistory")]
        public IActionResult PlacementHistory() => View();

        [HttpGet("/StudChangePass")]
        [HttpGet("/Student/ChangePassword")]
        public IActionResult ChangePassword() => View();

        [HttpGet("/StudForgotPassword")]
        [HttpGet("/Student/ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpGet("/StudLogout")]
        public IActionResult Logout() => RedirectToAction("Index", "Home");
    }
}
