using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers
{
    public class ScheduleController : Controller
    {
        public IActionResult Calendar()
        {
            return View();
        }
    }
}
