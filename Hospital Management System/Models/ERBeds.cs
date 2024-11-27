using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace Hospital_Management_System.Models
{
    public class ERBeds
    {
        [Key]
        public int Bed_Number { get; set; }
        public string? Status { get; set; }
    }
}
