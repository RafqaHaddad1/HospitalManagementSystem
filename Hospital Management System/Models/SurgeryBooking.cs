using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class SurgeryBooking
    {
        [Key]
        public int BookingID { get; set; } 
        public int? OR_ID { get; set; }
        public int? PatientID    { get; set; }
        public DateOnly? Date {  get; set; }
        public TimeOnly? Start {  get; set; }
        public TimeOnly? End { get; set; }
        public int? AssignedDoctor {  get; set; }
        public string? TypeOfSurgery { get; set; } 
        public string? Staff { get; set; }
    }
}
