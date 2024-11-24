using Hospital_Management_System.Database;
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

        public IActionResult Index()
        {
            return View();
        }

    }
}
