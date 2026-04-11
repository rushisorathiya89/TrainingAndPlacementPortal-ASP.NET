using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrainingAndPlacementPortal.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult AppliedStudents()
        {
            return View();
        }
    }
}
