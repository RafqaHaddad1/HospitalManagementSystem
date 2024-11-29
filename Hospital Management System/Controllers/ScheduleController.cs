using Hospital_Management_System.Database;
using Hospital_Management_System.Helper;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Entity;

namespace Hospital_Management_System.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly HospitalDbContext _dbContext;
        public ScheduleController(ILogger<ScheduleController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }

        public IActionResult Calendar()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events =  (from s in _dbContext.Schedules
                                    join emp in _dbContext.Staff
                                    on s.StaffID equals emp.StaffID
                                    select new
                                    {
                                        id = s.EventID, // Event ID
                                        title = emp.Name, // Staff name
                                        start = s.Start, // Start time from Schedule
                                        end = s.End, // End time from Schedule
                                        staffID = s.StaffID, // Staff ID from Schedule
                                        date = s.Date,
                                    }).ToList();

                return Json(new
                {
                    success = true,
                    model = events
                });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEventByDepAndRole(int depId, string role)
        {
            try
            {
                var events = (from s in _dbContext.Schedules.Where(s => s.DepartmentID == depId && s.Role == role) 
                              join emp in _dbContext.Staff
                              on s.StaffID equals emp.StaffID
                              select new
                              {
                                  id = s.EventID, // Event ID
                                  title = emp.Name, // Staff name
                                  start = s.Start, // Start time from Schedule
                                  end = s.End, // End time from Schedule
                                  staffID = s.StaffID, // Staff ID from Schedule
                                  date = s.Date,
                              }).ToList();

                return Json(new
                {
                    success = true,
                    model = events
                });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEventById(int id)
        {
            try
            {
                var events =  (from s in _dbContext.Schedules.Where(s => s.EventID == id)
                                    join emp in _dbContext.Staff on s.StaffID equals emp.StaffID
                                    join dept in _dbContext.Department on s.DepartmentID equals dept.DepartmentID // Assuming the Departments table has DepartmentID and Name
                                    select new
                                    {
                                        id = s.EventID,
                                        departmentID = s.DepartmentID,
                                        departmentName = dept.DepartmentName, // Department name
                                        title = emp.Name, // Staff name
                                        start = s.Start, // Start time from Schedule
                                        end = s.End, // End time from Schedule
                                        date= s.Date,
                                        staffID = s.StaffID, // Staff ID from Schedule
                                        role= s.Role,
                                    }).FirstOrDefault();

                return Json(new
                {
                    success = true,
                    model = events
                });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddEvent(Schedules model)
        {
            try
            {
                _dbContext.Schedules.Add(model);
                await _dbContext.SaveChangesAsync();  // Ensure to await the asynchronous call

                return Json(new
                {
                    success = true,
                    model = model
                });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEvent( Schedules updatedEvent)
        {
            
            try
            {
                var existingEvent = await _dbContext.Schedules.FindAsync(updatedEvent.EventID);
                if (existingEvent == null)
                {
                    _logger.LogWarning("Event with ID {EventID} not found.", updatedEvent.EventID);
                    return Json(new { success = false, message = "Event not found" });
                }

                _dbContext.Entry(existingEvent).CurrentValues.SetValues(updatedEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Event with ID {EventID} updated successfully.", updatedEvent.EventID);
                return Json(new { success = true, message = "Event updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the event with ID {EventID}", updatedEvent.EventID);
                return Json(new { success = false, message = "Error updating event", exception = ex.Message });
            }
        }

    }
}
