using Microsoft.AspNetCore.Mvc;

namespace TPPortal.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult ForgotPassword() => View();
        public IActionResult ChangePassword() => View();
        public IActionResult ResetPassword() => View();
    }
}
