using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class Login
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; } 
        public int StaffID { get; set; }
        public string Role {  get; set; }
    }
}
