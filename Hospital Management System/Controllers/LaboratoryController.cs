using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers
{
    public class LaboratoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
