using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;

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
        public async Task<IActionResult> AllLabs(string testType = null, string status = null)
        {
            // Query the Laboratories and join with Staff based on RequestedBy
            var LabsQuery = from lab in _dbContext.Laboratory
                            join staff in _dbContext.Staff on lab.RequestedBy equals staff.StaffID into staffJoin
                            from staff in staffJoin.DefaultIfEmpty() // Left join so it handles cases where no matching staff is found
                            select new
                            {
                                lab.ResultID,
                                lab.PatientID,
                                lab.TestType,
                                lab.SampleSubmitted,
                                lab.RequestedDate,
                                lab.ResultDate,
                                lab.Status,
                                lab.RequestedBy,
                                StaffName = staff != null ? staff.Name : null // Staff Name or null if no matching staff
                            };

            // Apply filtering based on the provided parameters
            if (!string.IsNullOrEmpty(testType))
            {
                LabsQuery = LabsQuery.Where(l => l.TestType == testType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                LabsQuery = LabsQuery.Where(l => l.Status == status);
            }
      
            // Execute the query and get the result
            var LabsWithStaff = await LabsQuery.ToListAsync();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = LabsWithStaff,
                });
            }

            // Return the view for normal (non-AJAX) requests
            return View();
            
        }


        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> RequestLab([FromBody] Laboratory model)
        {
            _logger.LogInformation("Model received: {@model}", model);
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {
                model.Status = "Pending";


                // If RequestedDate is not null, convert it to local time
                if (model.RequestedDate.HasValue)
                {
                    DateTime requestedDate = model.RequestedDate.Value; // Use Value because it's a nullable DateTime
                    DateTime localTime = requestedDate.ToLocalTime();   // Convert UTC time to local time
                    model.RequestedDate = localTime;  // Save the local time back to the model
                }

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
            // Fetch the lab record
            var lab = await _dbContext.Laboratory.FirstOrDefaultAsync(e => e.ResultID == id);

            // Check if lab record was found
            if (lab == null)
            {
                return Json(new { success = false, message = "Lab record not found." });
            }

            // Fetch the staff record based on RequestedBy
            var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.StaffID == lab.RequestedBy);
            var patient = await _dbContext.Patient.FirstOrDefaultAsync(p => p.PatientID == lab.PatientID);
            
            // Return the lab and staff data
            return Json(new
            {
                success = true,
                model = lab,
                staff = staff,
                patient = patient,
            });
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SubmitResults([FromForm]Laboratory model, [FromForm]IFormFileCollection Files)
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
                model.RequestedDate = oldLab.RequestedDate;
                model.TestType = oldLab.TestType;
                model.RequestedBy = oldLab.RequestedBy;
                model.SampleSubmitted = oldLab.SampleSubmitted;
                // Update values from the model
                _dbContext.Entry(oldLab).CurrentValues.SetValues(model);
              
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
                model.ResultFilePath = pathsString;
                oldLab.ResultFilePath = model.ResultFilePath;
                oldLab.Status = "Completed";
                _logger.LogInformation($"File paths: {model.ResultFilePath}");
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
                return View("AllLabs");
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
            var LabsWithStaff = await (from lab in _dbContext.Laboratory
                                       join staff in _dbContext.Staff on lab.RequestedBy equals staff.StaffID into staffJoin
                                       from staff in staffJoin.DefaultIfEmpty() // Left join so it handles cases where no matching staff is found
                                       select new
                                       {
                                           lab.ResultID,
                                           lab.PatientID,
                                           lab.TestType,
                                           lab.SampleSubmitted,
                                           lab.RequestedDate,
                                           lab.ResultDate,
                                           lab.Status,
                                           lab.RequestedBy,
                                           StaffName = staff != null ? staff.Name : null // Staff Name or null if no matching staff
                                       }).ToListAsync();

            return Json(new
            {
                success = true,
                model = LabsWithStaff,
            });
        }
        [HttpGet]
        public async Task<IActionResult> LabByDr(int id)
        {
            var lab = _dbContext.Laboratory.Where(e => e.RequestedBy== id).ToListAsync();
            return Json(new
            {
                success = true,
                model = lab,
            });
        }
        [HttpGet]
        public async Task<IActionResult> LabByPatient(int id)
        {
            var lab = _dbContext.Laboratory.Where(e => e.PatientID == id).ToListAsync();
            return Json(new
            {
                success = true,
                model = lab,
            });
        }
    }
}
