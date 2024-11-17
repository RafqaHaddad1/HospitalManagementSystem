using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace Hospital_Management_System.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public int? DepartmentHeadID { get; set; }
    }
}
