using Microsoft.AspNetCore.Mvc;

namespace TrainingAndPlacementPortal.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        [HttpGet("/Home")]
        [HttpGet("/Home/Index")]
        public IActionResult Index() => View();

        [HttpGet("/Register")]
        [HttpGet("/Home/Register")]
        public IActionResult Register() => View();

        [HttpGet("/SubmitJd")]
        [HttpGet("/Home/SubmitJd")]
        public IActionResult SubmitJd() => View();
    }
}
