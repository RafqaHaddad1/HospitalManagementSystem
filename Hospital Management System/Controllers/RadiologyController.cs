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
        public async Task<IActionResult> AllImages(string testType = null, string status = null)
        {
            // Build the query without executing it
            var imagequery = from img in _dbContext.RadiologyImages
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
                             };

            // Apply filtering based on parameters before executing the query
            if (!string.IsNullOrEmpty(testType))
            {
                imagequery = imagequery.Where(l => l.ImageType == testType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                imagequery = imagequery.Where(l => l.Status == status);
            }

            // Execute the query with the filters
            var imagesWithStaff = await imagequery.ToListAsync();

            // Check if the request is an AJAX request
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = imagesWithStaff,
                });
            }

            // Return the view for non-AJAX requests
            return View();
        }


        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> RequestImage([FromBody] RadiologyImages model)
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
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SubmitResults([FromForm] RadiologyImages model, [FromForm] IFormFileCollection Files)
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
                var oldImg = await _dbContext.RadiologyImages.FirstOrDefaultAsync(d => d.ImageID == model.ImageID);
                if (oldImg == null)
                {
                    return Json(new { success = false, message = "Lab not found" });
                }
                model.RequestedDate = oldImg.RequestedDate;
                model.ImageType = oldImg.ImageType;
                model.RequestedBy = oldImg.RequestedBy;
        
                // Update values from the model
                _dbContext.Entry(oldImg).CurrentValues.SetValues(model);

                model.Status = "Completed";
                var paths = new List<string>();
                foreach (var file in Files)
                {
                    if (file != null && file.Length > 0)
                    {
                        Console.Write(file);
                        // Specify the directory path
                        string uploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FilesUpload");

                        // Ensure directory exists
                        if (!Directory.Exists(uploadsDirectoryPath))
                        {
                            Directory.CreateDirectory(uploadsDirectoryPath);
                        }

                        var fileName = Path.GetFileName(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                        var fullPath = Path.Combine(uploadsDirectoryPath, uniqueFileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add the relative path to the list
                        var relativePath = $"/FilesUpload/{uniqueFileName}";
                        Console.Write(relativePath);
                        paths.Add(relativePath);
                    }
                }

                // Set the model's Files property after the loop
                var pathsString = string.Join(";", paths);
                model.ImagePath = pathsString;
                oldImg.ImagePath = model.ImagePath;
                oldImg.Status = "Completed";
                oldImg.ResultDate = DateTime.Now;   
                _logger.LogInformation($"File paths: {model.ImagePath}");
                _logger.LogInformation($"Status: {model.Status}");
                // Save the changes asynchronously
                await _dbContext.SaveChangesAsync();
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        model = model,
                    });
                }
                return View("AllImages");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating images");
                return Json(new { success = false, message = "Error updating images", exception = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImageByID(int id)
        {
            var image = await _dbContext.RadiologyImages.FirstOrDefaultAsync(e => e.ImageID == id);
            var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.StaffID == image.RequestedBy);
            var patient = await _dbContext.Patient.FirstOrDefaultAsync(p => p.PatientID == image.PatientID);
            return Json(new
            {
                success = true,
                model = image,
                Staff = staff,
                patient = patient,
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

        [HttpGet]
        public async Task<IActionResult> ImageByDr(int id)
        {
            var image = await _dbContext.RadiologyImages.FirstOrDefaultAsync(e => e.RequestedBy == id);
            var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.StaffID == image.RequestedBy);
            return Json(new
            {
                success = true,
                model = image,
                Staff = staff,
            });
        }

        [HttpGet]
        public async Task<IActionResult> ImageByPatient(int id)
        {
            var image = await _dbContext.RadiologyImages.FirstOrDefaultAsync(e => e.RequestedBy == id);
            var patient = await _dbContext.Patient.FirstOrDefaultAsync(s => s.PatientID == image.PatientID);
            return Json(new
            {
                success = true,
                model = image,
                Staff = patient,
            });
        }
        [HttpGet]
        public async Task<IActionResult> ImageByType(string type)
        {
            var image = await _dbContext.RadiologyImages.FirstOrDefaultAsync(e => e.ImageType == type);
            var patient = await _dbContext.Patient.FirstOrDefaultAsync(s => s.PatientID == image.PatientID);
            return Json(new
            {
                success = true,
                model = image,
                Staff = patient,
            });
        }
    }
}
