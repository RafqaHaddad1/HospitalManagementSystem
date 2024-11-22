using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class RadiologyImages
    {
        [Key]
        public int ImageID { get; set; }
        public int? PatientID { get; set; } 
        public int?  RequestedBy { get; set; } 
        public string? ImagePath { get; set; } 
        public string? Notes { get; set; }
        public string? ImageType  { get; set; }
        public string? Status { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? ResultDate { get; set; }

    }
}
