namespace Hospital_Management_System.Models
{
    public class Patient
    {
        public int Patient_ID { get; set; }
        public string FullName {get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactInfo { get; set; } 
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string MedicalHistory { get; set; } 
        public string Medications { get; set; }
        public string FamilyMedicalHistory { get; set; } 
        public string BloodType { get; set; }
        public string Allergies { get; set; }
        public int AssignedDoctorID { get; set; }
    }
}
