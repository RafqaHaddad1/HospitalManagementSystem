using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    public class ERController : Controller
    {
        private readonly ILogger<ERController> _logger;
        private readonly HospitalDbContext _dbContext;

        public ERController(ILogger<ERController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ViewPatients()
        {
            // Fetch patients from the database where DepartmentID is 1
            var patients = _dbContext.Patient
                                     .Where(p => p.DepartmentID == 1)
                                     .Select(p => new
                                     {
                                         PatientID = p.PatientID,
                                         FullName = p.FullName,
                                         Status = p.status,
                                         AssignedDoctorID = p.AssignedDoctorID,
                                         Addmission_Date_ER = p.Addmission_Date_ER,
                                         BedNumber = p.BedNumber,
                                         // Fetch the doctor based on AssignedDoctorID
                                         Doctor = _dbContext.Staff
                                                             .Where(d => d.StaffID == p.AssignedDoctorID)
                                                             .Select(d => new
                                                             {
                                                                 DoctorID = d.StaffID,
                                                                 DoctorName = d.Name
                                                             }).FirstOrDefault()
                                     }).ToList();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = patients
                });
            }

            // Return the view for normal (non-AJAX) requests
            return View();
        }

        public IActionResult AddPatient()
        {
            return View();
        }
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AddPatientToER(Patient model, IFormFileCollection Files)
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
                model.Addmission_Date_ER = DateTime.Now;
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
        [HttpGet]
        public async Task<ActionResult> AvailableBeds()
        {

            var available = _dbContext.ERBeds.Where(eb => eb.Status == "Available").ToList();

            return Json(new
            {
                success = true,
                model = available,
            });
        }
        [HttpGet]
        public async Task<ActionResult> PatientByIDInER(int id)
        {
            // Fetch the patient by ID and department
            var patient = await _dbContext.Patient
                                          .Where(p => p.PatientID == id && p.DepartmentID == 1)
                                          .Select(p => new
                                          {
                                              Patient = p, // Return the full patient object
                                                           // Fetch the doctor based on AssignedDoctorID
                                              Doctor = _dbContext.Staff
                                                                 .Where(d => d.StaffID == p.AssignedDoctorID)
                                                                 .FirstOrDefault()
                                          })
                                          .FirstOrDefaultAsync();

            if (patient == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Patient not found"
                });
            }

            return Json(new
            {
                success = true,
                model = new
                {
                    Patient = patient.Patient,  // Entire patient object
                    Doctor = patient.Doctor     // Associated doctor
                }
            });
        }

        public IActionResult EditPatient()
        {
            return View();
        }
    }
}
