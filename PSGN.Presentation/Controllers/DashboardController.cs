using Microsoft.AspNetCore.Mvc;

namespace PSGN.Presentation.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
