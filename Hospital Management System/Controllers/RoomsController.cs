using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers
{
    public class RoomsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
