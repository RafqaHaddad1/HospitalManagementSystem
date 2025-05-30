﻿using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace Hospital_Management_System.Models
{
    public class Staff
    {
        [Key]
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
        public string? EmploymentType { get; set; }
        public string? FilePath{ get; set; }
        public string? Password { get; set; }
        public string? Username { get; set; }
    }
}
