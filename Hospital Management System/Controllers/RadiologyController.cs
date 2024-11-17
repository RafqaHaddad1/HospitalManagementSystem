using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Hospital_Management_System.Controllers
{
    public class RadiologyController : Controller
    {
        private readonly ILogger<RadiologyController> _logger;
        private readonly HospitalDbContext _dbContext;

        public RadiologyController(ILogger<RadiologyController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AllImages()
        {
            //var images = await _dbContext.RadiologyImages.ToListAsync(); 
            //if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            //{
            //    return Json(new
            //    {
            //        success = true,
            //        model = images,
            //    });
            //}
            // Return the view for normal (non-AJAX) requests
            return View();
        }


        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> RequesImage([FromBody] RadiologyImages model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {

                _dbContext.RadiologyImages.Add(model);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Added successfully");

                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        info = model,

                    });
                }
                // Return the view for normal (non-AJAX) requests
                return View("AllImages");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting request");
                return Json(new { success = false, message = "Error submitting request", exception = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ImageByID(int id)
        {
            var image = _dbContext.RadiologyImages.Where(e => e.ImageID == id);
            return Json(new
            {
                success = true,
                model = image,
            });
        }


        [HttpGet]
        public async Task<IActionResult> ImageByStatus(string status)
        {
            var image = _dbContext.Laboratory.Where(e => e.Status == status).ToListAsync();
            return Json(new
            {
                success = true,
                model = image,
            });
        }
    }
}
