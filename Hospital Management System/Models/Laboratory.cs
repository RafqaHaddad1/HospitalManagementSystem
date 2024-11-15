using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class Laboratory
    {
        [Key]
        public int ResultID { get; set; }
        public int? PatientID{ get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? ResultDate { get; set; }
        public string? TestType { get; set; }
        public string? Notes { get; set; }
        public int? RequestedBy { get; set; }
        public string? ResultFilePath { get; set; }
        public string? Status  { get; set; }
        public string? SampleSubmitted { get; set; }
    }
}
