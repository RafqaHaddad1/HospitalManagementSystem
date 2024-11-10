using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers
{
    public class RadiologyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
