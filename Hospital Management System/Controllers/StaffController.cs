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
        public async Task<IActionResult> Staffs(string role = null, string departmentName = null)
        {
            // Fetch staff and department data
            var staffQuery = from s in _dbContext.Staff
                             join d in _dbContext.Department on s.Department equals d.DepartmentID into departmentJoin
                             from department in departmentJoin.DefaultIfEmpty() // Left join to include staff without departments
                             select new
                             {
                                 s.StaffID,
                                 s.Name,
                                 s.PhoneNumber,
                                 s.Role,
                                 DepartmentID = s.Department,
                                 DepartmentName = department != null ? department.DepartmentName : null
                             };

            // Apply filtering based on role and department name
            if (!string.IsNullOrEmpty(role))
            {
                staffQuery = staffQuery.Where(s => s.Role == role);
            }

            if (!string.IsNullOrEmpty(departmentName))
            {
                staffQuery = staffQuery.Where(s => s.DepartmentName.Contains(departmentName));
            }

            // Execute the query to get the filtered staff with department names
            var staffWithDepartments = await staffQuery.ToListAsync();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = staffWithDepartments,
                });
            }

            // Send the model to the view for normal (non-AJAX) requests
            return View();

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
                _logger.LogInformation("model id: ", model.StaffID);
                var login = new Login
                {
                    Username = model.Username,  // Assuming 'Username' is a field in Staff
                    Password = pass,  // Already hashed password
                    Role = model.Role,  // Assuming 'Role' is a field in Staff, or specify it as needed
                    StaffID = model.StaffID// Assuming 'Id' is the primary key in the Staff model
                };

                // Add the login details to the Login table
                _dbContext.Login.Add(login);
                await _dbContext.SaveChangesAsync();
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
        public async Task<IActionResult> StaffByRole(string role,int department)
        {
            // Fetch staff by id
            var staff = await _dbContext.Staff
             .Where(e => e.Role == role && e.Department == department)
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
