namespace Hospital_Management_System.Models
{
    public class RoomAssignment
    {
        public int AssignmentID { get; set; } 
        public int RoomID { get; set; } 
        public int PatientID { get; set; } 
        public DateTime AdmissionDate { get; set; } 
        public DateTime DischargeDate { get; set; }

    }
}
