using Microsoft.Identity.Client;
using System.Security.Policy;

namespace Hospital_Management_System.Models
{
    public class Staff
    {
        public int StaffID { get; set; } 
        public string? Name { get; set; } 
        public string? Gender { get; set; } 
        public DateOnly? DateOfBirth { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string? Address { get; set; } 
        public string? Qualifications { get; set; } 
        public string? Role   { get; set; }
        public int? Department { get; set; } 
        public string? DoctorLicenseNumber { get; set; } 
    }
}
