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
        public async Task<IActionResult> ViewPatients(string dr = null, string status = null)
        {
            // Query the patients and join with Doctor (assuming there is a relation between patient and doctor)
            var patientsQuery = from patient in _dbContext.Patient
                                join doctor in _dbContext.Staff on patient.AssignedDoctorID equals doctor.StaffID into doctorJoin
                                from doctor in doctorJoin.DefaultIfEmpty() // Left join so it handles cases where no matching doctor is found
                                where patient.DepartmentID == 1 // Assuming you're filtering by a specific DepartmentID (e.g., 1)
                                select new
                                {
                                    patient.PatientID,
                                    patient.FullName,
                                    patient.Status,
                                    patient.AssignedDoctorID,
                                    Addmission_Date_ER = patient.Addmission_Date_ER,
                                    BedNumber = patient.BedNumber,
                                    // Doctor name (or null if no matching doctor)
                                    DoctorName = doctor != null ? doctor.Name : null,
                                    DoctorID = doctor != null ? doctor.StaffID : (int?)null
                                };

            // Apply filtering based on the provided parameters
            if (!string.IsNullOrEmpty(dr))
            {
                patientsQuery = patientsQuery.Where(p => p.DoctorName == dr);
            }

            if (!string.IsNullOrEmpty(status))
            {
                patientsQuery = patientsQuery.Where(p => p.Status == status);
            }

            // Execute the query and get the result
            var patients = await patientsQuery.ToListAsync();

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



        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> EditERPatient(Patient model, IFormFileCollection Files)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            try
            {
                // Fetch the existing patient from the database
                var existingPatient = await _dbContext.Patient.FindAsync(model.PatientID);

                if (existingPatient == null)
                {
                    return Json(new { success = false, message = "Patient not found" });
                }

                // Set the updated values to the existing patient
                _dbContext.Entry(existingPatient).CurrentValues.SetValues(model);

                // Handle file uploads if any
                if (Files != null && Files.Any())
                {
                    var paths = new List<string>();
                    foreach (var file in Files)
                    {
                        if (file.Length > 0)
                        {
                            string uploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FilesUpload");

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

                            var relativePath = $"/FilesUpload/{uniqueFileName}";
                            paths.Add(relativePath);
                        }
                    }

                    if (paths.Any())
                    {
                        existingPatient.Files = string.Join(";", paths); // Update Files if new files were uploaded
                    }
                }

                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Patient updated successfully");

                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        info = existingPatient, // Returning updated patient information
                    });
                }

                // Return the view for normal (non-AJAX) requests
                return View("ViewPatients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient");
                return Json(new { success = false, message = "Error updating patient", exception = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> DischargePatient(int id)
        {
            // Fetch the patient by ID
            var patient = await _dbContext.Patient.Where(p => p.PatientID == id).FirstOrDefaultAsync();

            // Check if the patient exists
            if (patient == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Patient not found"
                });
            }

            // Update patient status and discharge date
            patient.Status = "Discharged";  // Assign the "Discharged" status
            patient.Discharge_Date = DateTime.Now;

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();

            // Return success response
            return Json(new
            {
                success = true,
                model = patient
            });
        }


    }
}
