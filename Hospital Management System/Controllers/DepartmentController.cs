using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ILogger<DepartmentController> _logger;
        private readonly HospitalDbContext _dbContext;
        public DepartmentController(ILogger<DepartmentController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AllDepartments()
        {
            var departments = await _dbContext.Department.ToListAsync(); // Use async version
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    model = departments,
                });
            }
            // Return the view for normal (non-AJAX) requests
            return View("Departments");
        }
        
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AddDepartment(Department model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            try
            {

                _dbContext.Department.Add(model);
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
                return View("Departments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding department");
                return Json(new { success = false, message = "Error adding department", exception = ex.Message });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> DepartmentByID(int id)
        {
            var department = _dbContext.Department.Where(e => e.DepartmentID == id);
            return Json(new
            {
                success = true,
                model = department,
            });
        }
        
        [HttpPut]
        public async Task<IActionResult> UpdateDepartment(Department model)
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
                var oldDep = await _dbContext.Department.FirstOrDefaultAsync(d => d.DepartmentID == model.DepartmentID);
                if (oldDep == null)
                {
                    return Json(new { success = false, message = "Department not found" });
                }

                // Update values from the model
                _dbContext.Entry(oldDep).CurrentValues.SetValues(model);

                // Save the changes asynchronously
                await _dbContext.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    model = model,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                return Json(new { success = false, message = "Error updating department", exception = ex.Message });
            }
        }


    }
}
