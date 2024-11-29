using Hospital_Management_System.Database;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers
{
    public class ORController : Controller
    {
        private readonly ILogger<ORController> _logger;
        private readonly HospitalDbContext _dbContext;

        public ORController(ILogger<ORController> logger, HospitalDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }
        public IActionResult ORBooking()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOR()
        {
            try
            {
                var or = _dbContext.OperatingRoom.ToList();

                return Json(new
                {
                    success = true,
                    model = or
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
        public async Task<IActionResult> AddOR(OperatingRoom model)
        {
            try
            {
                _dbContext.OperatingRoom.Add(model);
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
        public async Task<IActionResult> AddEvent(SurgeryBooking model)
        {
            try
            {
                _dbContext.SurgeryBooking.Add(model);
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
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = (from s in _dbContext.SurgeryBooking
                              join emp in _dbContext.Staff
                              on s.AssignedDoctor equals emp.StaffID
                              select new
                              {
                                  id = s.BookingID, // Event ID
                                  title = emp.Name, // Staff name
                                  start = s.Start, // Start time from Schedule
                                  end = s.End, // End time from Schedule
                                  staffID = s.AssignedDoctor, // Staff ID from Schedule
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
        public async Task<IActionResult> GetEventByOR(int or)
        {
            try
            {
                var events = (from s in _dbContext.SurgeryBooking.Where(s => s.OR_ID == or)
                              join emp in _dbContext.Staff
                              on s.AssignedDoctor equals emp.StaffID
                              select new
                              {
                                  id = s.BookingID, // Event ID
                                  title = s.TypeOfSurgery,
                                  DrName = emp.Name, // Staff name
                                  start = s.Start, // Start time from Schedule
                                  end = s.End, // End time from Schedule
                                  staffID = s.AssignedDoctor, // Staff ID from Schedule
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
                var events = (from s in _dbContext.SurgeryBooking.Where(s => s.BookingID == id)
                              join emp in _dbContext.Staff on s.AssignedDoctor equals emp.StaffID
                                join pat in _dbContext.Patient on s.PatientID equals pat.PatientID
                              select new
                              {
                                  id = s.BookingID,
                                  
                                  title = emp.Name, // Staff name
                                  start = s.Start, // Start time from Schedule
                                  end = s.End, // End time from Schedule
                                  date = s.Date,
                                  staffID = s.AssignedDoctor, // Staff ID from Schedule
                                  orid= s.OR_ID,
                                  typeofsurgery = s.TypeOfSurgery,
                                  patientid = s.PatientID,
                                  patientName = pat.FullName, 
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
        [HttpGet]
        public async Task<IActionResult> GetEventByPatient(int id)
        {
            try
            {
                var events = (from s in _dbContext.SurgeryBooking.Where(s => s.PatientID == id)
                              join emp in _dbContext.Staff on s.AssignedDoctor equals emp.StaffID
                              join pat in _dbContext.Patient on s.PatientID equals pat.PatientID
                              select new
                              {
                                  id = s.BookingID,
                                  title = emp.Name, // Staff name
                                  start = s.Start, // Start time from Schedule
                                  end = s.End, // End time from Schedule
                                  date = s.Date,
                                  staffID = s.AssignedDoctor, // Staff ID from Schedule
                                  orid = s.OR_ID,
                                  typeofsurgery = s.TypeOfSurgery,
                                  patientid = s.PatientID,
                                  patientName = pat.FullName,
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
        [HttpPost]
        public async Task<IActionResult> UpdateEvent(SurgeryBooking updatedEvent)
        {

            try
            {
                var existingEvent = await _dbContext.SurgeryBooking.FindAsync(updatedEvent.BookingID);
                if (existingEvent == null)
                {
                    _logger.LogWarning("Event with ID {EventID} not found.", updatedEvent.BookingID);
                    return Json(new { success = false, message = "Event not found" });
                }

                _dbContext.Entry(existingEvent).CurrentValues.SetValues(updatedEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Event with ID {EventID} updated successfully.", updatedEvent.BookingID);
                return Json(new { success = true, message = "Event updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the event with ID {EventID}", updatedEvent.BookingID);
                return Json(new { success = false, message = "Error updating event", exception = ex.Message });
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteEvent (int id)
        {
            try
            {
               var model = _dbContext.SurgeryBooking.Find(id);
                _dbContext.SurgeryBooking.Remove(model);
                _dbContext.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                   
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
    }
}
