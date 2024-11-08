namespace Hospital_Management_System.Models
{
    public class SurgeryBooking
    {
        public int BookinID { get; set; } 
        public int OR_ID { get; set; }
        public int PatientID    { get; set; }
        public DateTime Start {  get; set; }
        public DateTime End { get; set; }
        public int AssignedDoctor {  get; set; }
        public string TypeOfSurgery { get; set; } 
        public string Staff { get; set; }
    }
}
