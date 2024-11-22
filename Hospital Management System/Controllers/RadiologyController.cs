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
            var imagesWithStaff = await (from img in _dbContext.RadiologyImages
                                         join staff in _dbContext.Staff on img.RequestedBy equals staff.StaffID into staffJoin
                                         from staff in staffJoin.DefaultIfEmpty() // Left join to handle missing staff
                                         select new
                                         {
                                             img.ImageID,
                                             img.PatientID,
                                             img.ImageType,
                                             img.RequestedDate,
                                             img.Status,
                                             img.ResultDate,
                                             img.RequestedBy,
                                             StaffName = staff != null ? staff.Name : null // Staff Name or null if no matching staff
                                         }).ToListAsync();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = imagesWithStaff,
                });
            }
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
                model.Status = "Pending";
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
            var image = await _dbContext.RadiologyImages.FirstOrDefaultAsync(e => e.ImageID == id);
            var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.StaffID == image.RequestedBy);
            return Json(new
            {
                success = true,
                model = image,
                Staff = staff,
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
