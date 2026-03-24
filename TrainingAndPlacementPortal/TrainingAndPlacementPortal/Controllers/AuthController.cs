using Microsoft.AspNetCore.Mvc;

namespace TrainingAndPlacementPortal.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet("/Authentication/login")]
        [HttpGet("/Auth/Login")]
        public IActionResult Login() => View();

        [HttpGet("/ForgotPassword")]
        [HttpGet("/Auth/ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpGet("/Authentication/change-password")]
        [HttpGet("/Auth/ChangePassword")]
        public IActionResult ChangePassword() => View();

        [HttpGet("/Authentication/reset-password")]
        [HttpGet("/Auth/ResetPassword")]
        public IActionResult ResetPassword() => View();
    }
}
