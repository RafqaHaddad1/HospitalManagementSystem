using Hospital_Management_System.Database;
using Hospital_Management_System.Helper;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
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
       
        public async Task<IActionResult> AllPatients(string dr = null, string status = null)
        {
            var patients = await _dbContext.Patient.ToListAsync();
            return Json(new
            {
                success = true,
                model = patients,
            });
        }
        //[HttpGet]
        //[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        //public async Task<IActionResult> AllPatients(string dr = null, string status = null)
        //{
        //    // Query the patients and join with Doctor (assuming there is a relation between patient and doctor)
        //    var patientsQuery = from patient in _dbContext.Patient
        //                        join doctor in _dbContext.Staff on patient.AssignedDoctorID equals doctor.StaffID into doctorJoin
        //                        from doctor in doctorJoin.DefaultIfEmpty() // Left join so it handles cases where no matching doctor is found
        //                        select new
        //                        {
        //                            patient.PatientID,
        //                            patient.FullName,
        //                            patient.DateOfBirth,
        //                            patient.Status,
        //                            patient.AssignedDoctorID,
        //                            DoctorName = doctor != null ? doctor.Name : null // Doctor Name or null if no matching doctor
        //                        };

        //    // Apply filtering based on the provided parameters
        //    if (!string.IsNullOrEmpty(dr))
        //    {
        //        patientsQuery = patientsQuery.Where(p => p.DoctorName == dr);
        //    }

        //    if (!string.IsNullOrEmpty(status))
        //    {
        //        patientsQuery = patientsQuery.Where(p => p.Status == status);
        //    }

        //    // Execute the query and get the result
        //    var patientsWithDoctors = await patientsQuery.ToListAsync();


        //        return Json(new
        //        {
        //            success = true,
        //            model = patientsWithDoctors,
        //        });



        //}

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

        [Authorize(Roles = "Admin,Doctor,Nurse")]
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> PatientAdmitted(string dr = null, string status = null)
        {
            // Query the patients and join with Doctor (assuming there is a relation between patient and doctor)
            var patientsQuery = from patient in _dbContext.Patient
                                join doctor in _dbContext.Staff on patient.AssignedDoctorID equals doctor.StaffID into doctorJoin
                                from doctor in doctorJoin.DefaultIfEmpty() // Left join so it handles cases where no matching doctor is found
                                join department in _dbContext.Department on patient.DepartmentID equals department.DepartmentID into departmentJoin
                                from department in departmentJoin.DefaultIfEmpty() // Left join for department
                                where patient.DepartmentID!= 1// Ensure patients have an admission date
                                select new
                                {
                                    patient.PatientID,
                                    patient.FullName,
                                    patient.Status,
                                    patient.AssignedDoctorID,
                                    Addmission_Date_Hospital = patient.Admission_Date_Hospital,
                                    BedNumber = patient.BedNumber,
                                    // Doctor name (or null if no matching doctor)
                                    DoctorName = doctor != null ? doctor.Name : null,
                                    DoctorID = doctor != null ? doctor.StaffID : (int?)null,
                                    // Department information (or null if no matching department)
                                    DepartmentName = department != null ? department.DepartmentName : null,
                                    DepartmentID = department != null ? department.DepartmentID : (int?)null,
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
            var patientsWithDoctorsAndDepartments = await patientsQuery.ToListAsync();

            return Json(new
            {
                success = true,
                model = patientsWithDoctorsAndDepartments,
            });
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
        [Authorize(Roles = "Admin,Doctor,Nurse")]
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

                // Set the updated values to the existing patient except for specific fields
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
                        existingPatient.Files = string.Join(";", paths); // Only update if new files were uploaded
                    }
                }

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
                        info = existingPatient, // Returning updated patient information
                    });
                }

                // Return the view for normal (non-AJAX) requests
                return View("Patients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient");
                return Json(new { success = false, message = "Error updating patient", exception = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Doctor,Nurse")]
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
