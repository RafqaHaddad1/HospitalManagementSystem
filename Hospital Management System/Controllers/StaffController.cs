using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.Security.Policy;

namespace Hospital_Management_System.Controllers
{
    public class StaffController : Controller
    {
        private readonly ILogger<StaffController> _logger;
        private readonly HospitalDbContext _dbContext;
        public StaffController(ILogger<StaffController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
           
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
            var staff = _dbContext.Staff.Where(e => e.StaffID == id);
            return Json(new
            {
                success = true,
                model = staff,
            });
        }
        public IActionResult AddStaff()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(Staff model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {

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
        public async Task<IActionResult> UpdateEmployee(Staff model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {
                var existingEmployee = await _dbContext.Staff.FindAsync(model.StaffID);
         
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

    }
}
