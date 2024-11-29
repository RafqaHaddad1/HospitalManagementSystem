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
        public IActionResult EditPatient()
        {
            return View();
        }
        public IActionResult AddPatient()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddPatient2(Patient model, IFormFileCollection Files)
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
                model.Admission_Date_Hospital = DateTime.Now;
                var room = model.BedNumber;
                var room1 = _dbContext.Room.FirstOrDefault(b => b.RoomID== room);
                room1.IsAvailable = false;
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
                return View("Patients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff");
                return Json(new { success = false, message = "Error adding staff", exception = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> PatientAdmitted()
        {
            var patients = _dbContext.Patient
                                      .Where(p => p.Admission_Date_Hospital != null)
                                      .Select(p => new
                                      {
                                          PatientID = p.PatientID,
                                          FullName = p.FullName,
                                          Status = p.Status,
                                          AssignedDoctorID = p.AssignedDoctorID,
                                          Addmission_Date_Hospital = p.Admission_Date_Hospital,
                                          BedNumber = p.BedNumber,
                                          // Fetch the doctor based on AssignedDoctorID
                                          Doctor = _dbContext.Staff
                                                              .Where(d => d.StaffID == p.AssignedDoctorID)
                                                              .Select(d => new
                                                              {
                                                                  DoctorID = d.StaffID,
                                                                  DoctorName = d.Name
                                                              }).FirstOrDefault(),
                                        Department = _dbContext.Department
                                                    .Where(d => d.DepartmentID == p.DepartmentID)
                                                    .Select(d=> new
                                                    {
                                                        DepartmentID = d.DepartmentID,
                                                        DepartmentName = d.DepartmentName,
                                                    }).FirstOrDefault(),
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
    
        [HttpGet]
        public async Task<IActionResult> GetAvailableRoom()
        {
            var rooms = await _dbContext.Room.Where(p => p.IsAvailable == true).ToListAsync();
            return Json(new
            {
                success = true,
                model = rooms,
            });
        }

        [HttpGet]
        public async Task<ActionResult> PatientByID(int id)
        {
            // Fetch the patient by ID and department
            var patient = await _dbContext.Patient
                                          .Where(p => p.PatientID == id)
                                          .Select(p => new
                                          {
                                              Patient = p, // Return the full patient object
                                                           // Fetch the doctor based on AssignedDoctorID
                                              Doctor = _dbContext.Staff
                                                                 .Where(d => d.StaffID == p.AssignedDoctorID)
                                                                 .FirstOrDefault(),
                                            Department = _dbContext.Department
                                                            .Where (d => d.DepartmentID == p.DepartmentID)  
                                                            .FirstOrDefault(),

                                              Room = _dbContext.Room
                                                .Where(d=> d.RoomID == p.BedNumber && p.Admission_Date_Hospital !=null)
                                                .FirstOrDefault(),
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
                    Doctor = patient.Doctor,
                    Room = patient.Room,// Associated doctor
                    Department = patient.Department,
                }
            });
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> EditPatient(Patient model, IFormFileCollection Files)
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

                var paths = new List<string>();
                foreach (var file in Files)
                {
                    if (file != null && file.Length > 0)
                    {
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
                        paths.Add(relativePath);
                    }
                }

                // Set the Files property of the patient model
                var pathsString = string.Join(";", paths);
                model.Files = pathsString;
                model.Addmission_Date_ER = existingPatient.Admission_Date_Hospital;
                model.BloodType = existingPatient.BloodType;
                model.Gender = existingPatient.Gender;
                // Update the existing patient's properties
                _dbContext.Entry(existingPatient).CurrentValues.SetValues(model);

                // Update the bed status based on the bed number
                var bed = _dbContext.ERBeds.FirstOrDefault(b => b.Bed_Number == model.BedNumber);
                if (bed != null)
                {
                    bed.Status = "Unavailable";
                }

                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Patient updated successfully");

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
