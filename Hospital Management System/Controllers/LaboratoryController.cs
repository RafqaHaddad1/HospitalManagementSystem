using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    public class LaboratoryController : Controller
    {
        private readonly ILogger<LaboratoryController> _logger;
        private readonly HospitalDbContext _dbContext;

        public LaboratoryController(ILogger<LaboratoryController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AllLabs()
        {
            var Labs = await _dbContext.Laboratory.ToListAsync(); // Use async version
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = Labs,
                });
            }
            // Return the view for normal (non-AJAX) requests
            return View();
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> RequestLab([FromBody] Laboratory model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {

                _dbContext.Laboratory.Add(model);
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
                return View("AllLabs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting request");
                return Json(new { success = false, message = "Error submitting request", exception = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LabByID(int id)
        {
            var lab = _dbContext.Laboratory.Where(e => e.ResultID == id);
            return Json(new
            {
                success = true,
                model = lab,
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLab([FromBody]Laboratory model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid model state",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                // Fetch the existing department record
                var oldLab = await _dbContext.Laboratory.FirstOrDefaultAsync(d => d.ResultID == model.ResultID);
                if (oldLab == null)
                {
                    return Json(new { success = false, message = "Lab not found" });
                }

                // Update values from the model
                _dbContext.Entry(oldLab).CurrentValues.SetValues(model);

                // Save the changes asynchronously
                await _dbContext.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    model = model,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab");
                return Json(new { success = false, message = "Error updating lab", exception = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LabByStatus(string status)
        {
            var lab = _dbContext.Laboratory.Where(e => e.Status == status).ToListAsync();
            return Json(new
            {
                success = true,
                model = lab,
            });
        }

    }
}
