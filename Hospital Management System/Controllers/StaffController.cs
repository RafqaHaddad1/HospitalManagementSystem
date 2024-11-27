using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Helper;

namespace Hospital_Management_System.Controllers
{
    public class StaffController : Controller
    {
        private readonly ILogger<StaffController> _logger;
        private readonly HospitalDbContext _dbContext;
        private readonly Password _password;
        public StaffController(ILogger<StaffController> logger, HospitalDbContext dbContext, Password password)
        {
            _logger = logger;
            _dbContext = dbContext;
            _password = password;
        }
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Staffs()
        {
            // Fetch staff and department data
            var staff = _dbContext.Staff.ToList();
            var departments = _dbContext.Department.ToList();

            var staffWithDepartments = staff.Select(s => new
            {
                StaffID = s.StaffID,
                Name = s.Name,
                phoneNumber = s.PhoneNumber,
                title = s.Role,
                DepartmentID = s.Department,
                DepartmentName = departments.FirstOrDefault(d => d.DepartmentID == s.Department)?.DepartmentName
            }).ToList();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = staffWithDepartments,
                });
            }

            // Send the model to the view for normal (non-AJAX) requests
            return View(staffWithDepartments);
        }

        [HttpGet]
        public async Task<IActionResult> StaffById(int id)
        {
            // Fetch staff by id
            var staff = await _dbContext.Staff
                .FirstOrDefaultAsync(e => e.StaffID == id);

            if (staff == null)
            {
                return NotFound(new { success = false, message = "Staff not found." });
            }

            // Fetch associated department
            var department = await _dbContext.Department
                .FirstOrDefaultAsync(d => d.DepartmentID == staff.Department);

            if (department == null)
            {
                return NotFound(new { success = false, message = "Department not found." });
            }

            // Return staff and department in JSON response
            return Json(new
            {
                success = true,
                model = new
                {
                    staff = staff,
                    department = department
                }
            });
        }
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(Staff model, IFormFileCollection Files)
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
                model.FilePath = pathsString;
                var pass = _password.HashPassword(model.Password);
                model.Password = pass;
                _dbContext.Staff.Add(model);
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
                return View("Staffs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff");
                return Json(new { success = false, message = "Error adding staff", exception = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(Staff model, IFormFileCollection Files)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {
                var existingEmployee = await _dbContext.Staff.FindAsync(model.StaffID);
                var existingPaths = existingEmployee.FilePath?.Split(';').ToList() ?? new List<string>();
                model.Password = existingEmployee.Password ;
                // Update the existing employee with new values (excluding files)
                _dbContext.Entry(existingEmployee).CurrentValues.SetValues(model);
                var newPaths = new List<string>();
                foreach (var file in Files)
                {
                    if (file != null && file.Length > 0)
                    {
                        _logger.LogInformation($"Processing file: {file.FileName}");


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

                        // Add the relative path to the new paths list
                        var relativePath = $"/FilesUpload/{uniqueFileName}";
                        _logger.LogInformation($"File saved at path: {relativePath}");
                        newPaths.Add(relativePath);
                    }
                }

                // Combine existing paths with new ones
                existingPaths.AddRange(newPaths);
                foreach (var file in existingPaths)
                {
                    Console.WriteLine(file);
                }

                // Update the employee's Files property with combined paths
                existingEmployee.FilePath = string.Join(";", existingPaths);
                Console.WriteLine(existingEmployee.FilePath);
               
                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Employee updated successfully");

                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        info = model,

                    });
                }
                // Return the view for normal (non-AJAX) requests
                return View("Staffs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff");
                return Json(new { success = false, message = "Error adding staff", exception = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AllDoctors()
        {
            var staff = _dbContext.Staff.Where(e => e.Role == "Doctor");
            return Json(new
            {
                success = true,
                model = staff,
            });
        }
        [HttpGet]
        public async Task<IActionResult> AllNurses()
        {
            var staff = _dbContext.Staff.Where(e => e.Role == "Nurse");
            return Json(new
            {
                success = true,
                model = staff,
            });
        }
        [HttpGet]
        public async Task<IActionResult> AllLabPersonel()
        {
            var staff = _dbContext.Staff.Where(e => e.Role == "Lab");
            return Json(new
            {
                success = true,
                model = staff,
            });
        }
        [HttpGet]
        public async Task<IActionResult> AllRadioPersonel()
        {
            var staff = _dbContext.Staff.Where(e => e.Role == "Radiology");
            return Json(new
            {
                success = true,
                model = staff,
            });
        }
   
        public IActionResult EditStaff()
        {
            return View("EditStaff");
        }

        [HttpGet]
        public async Task<IActionResult> StaffByRole(string role)
        {
            // Fetch staff by id
            var staff = await _dbContext.Staff
             .Where(e => e.Role == role)
             .Select(e => new
             {
                 e.Name,   // Assuming the property is 'Name' for staff name
                 e.StaffID      // Assuming the property is 'Id' for staff id
             })
             .ToListAsync();


            if (staff == null)
            {
                return NotFound(new { success = false, message = "Staff not found." });
            }

           

           
            // Return staff and department in JSON response
            return Json(new
            {
                success = true,
                model = new
                {
                    staff = staff
                   
                }
            });
        }

    }
}
