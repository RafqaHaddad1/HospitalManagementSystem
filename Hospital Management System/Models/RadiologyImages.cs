using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class RadiologyImages
    {
        [Key]
        public int ImageID { get; set; }
        public int? PatiendID { get; set; } 
        public int?  RequestedBy { get; set; } 
        public string? ImagePath { get; set; } 
        public string? Notes { get; set; }
        public string? Report  { get; set; }
        public string? Status { get; set; }

    }
}
