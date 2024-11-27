using Hospital_Management_System.Database;
using Hospital_Management_System.Helper;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly HospitalDbContext _dbContext;

        public PatientController(ILogger<PatientController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }
        public async Task<IActionResult> AllPatients()
        {
            var patients = await _dbContext.Patient.ToListAsync();
             return Json(new
            {
                success = true,
                model = patients,
            });
        }
        public async Task<IActionResult> PatientsByDrID(int id)
        {
            // Query to get only the PatientID and FullName for patients assigned to the specified doctor (id)
            var patients = await _dbContext.Patient
                                           .Where(p => p.AssignedDoctorID == id)
                                           .Select(p => new
                                           {
                                               p.PatientID,
                                               p.FullName
                                           })
                                           .ToListAsync();

            return Json(new
            {
                success = true,
                model = patients,
            });
        }

        public IActionResult Patients()
        {
            return View();
        }
        public async Task<IActionResult> AddPatient(Patient model, IFormFileCollection Files)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {
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
                model.Files = pathsString;
                var bednumber = model.BedNumber;
                var bed = _dbContext.ERBeds.FirstOrDefault(b => b.Bed_Number == bednumber);
                bed.Status = "Unavailable";
                _dbContext.Patient.Add(model);
             
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
                return View("ViewPatients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff");
                return Json(new { success = false, message = "Error adding staff", exception = ex.Message });
            }
        }
    }
}
