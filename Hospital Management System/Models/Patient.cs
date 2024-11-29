using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }
        public string? FullName {get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ContactInfo { get; set; } 
        public string? Address { get; set; }
        public string? EmergencyContactInfo { get; set; } 
        public string? MedicalHistory { get; set; } 
        public string? Medications { get; set; }
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public int? AssignedDoctorID { get; set; }
        public int? DepartmentID { get; set; }
        public string? Files { get; set; }
        public string? Status { get; set; }
        public int? BedNumber { get; set; }
        public DateTime? Addmission_Date_ER {  get; set; }
        public DateTime? Admission_Date_Hospital {  get; set; }
        public DateTime? Discharge_Date {  get; set; }
        public string? VitalSigns { get; set; }
        public string? SymptomsSigns { get; set; }
        public string? MedicationsGiven { get; set; }
        public string? Treatment { get; set; }
        public string? Overview { get; set; }

    }
}
